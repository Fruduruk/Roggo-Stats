using System.Diagnostics;
using BallchasingWrapper.Extensions;
using BallchasingWrapper.Interfaces;
using BallchasingWrapper.Models;
using Replay = BallchasingWrapper.Models.ReplayModels.Replay;

namespace BallchasingWrapper.BusinessLogic
{
    /// <summary>
    /// The replay collector is the smart guy in this program.
    /// It not only collects various replays asynchronously, it compares them, sorts out double replays and
    /// makes use of the replay cache to safe resources.
    /// </summary>
    public class ReplayCollector : IDisposable
    {
        private readonly ApiUrlCreator _urlCreator;
        private readonly IBallchasingApi _api;
        private IReplayCache _replayCache;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private bool _disposed;
        private readonly List<SimpleReplayDownloader> _downloaders;
        private readonly Task backgroundDownloadTask;

        public ReplayCollector(ApiUrlCreator urlCreator, IBallchasingApi api, IReplayCache replayCache)
        {
            _urlCreator = urlCreator;
            _api = api;
            _replayCache = replayCache;
            _cancellationTokenSource = new CancellationTokenSource();
            _downloaders = urlCreator.Urls.Select(url =>
                new SimpleReplayDownloader(_api, url, _replayCache)).ToList();
            backgroundDownloadTask = Task.WhenAll(_downloaders.Select(downloader =>
            {
                return Task.Run(async () =>
                {
                    await downloader.StartBackgroundDownloadAsync(_cancellationTokenSource.Token);
                });
            }));
        }

        public async Task<CollectReplaysResponse> CollectReplaysAsync(ILogger logger,
            CancellationToken cancellationToken)
        {
            logger.LogInformation("Started collecting replays.");

            var response = new CollectReplaysResponse();
            var sw = new Stopwatch();
            sw.Start();

            var replays = (await GetReplays(logger, cancellationToken)).ToList();

            sw.Stop();

            response.ElapsedMilliseconds = sw.ElapsedMilliseconds;

            if (cancellationToken.IsCancellationRequested)
            {
                logger.LogInformation("Cancelled collecting replays.");
                return response;
            }

            response.Replays = replays;
            
            logger.LogInformation("Finished collecting replays.");
            logger.LogDebugObject(new
            {
                ReplayCount = replays.Count,
                sw.ElapsedMilliseconds
            });
            logger.LogDebugObject(new
            {
                _api
            });

            return response;
        }

        private async Task<IEnumerable<Replay>> GetReplays(ILogger logger,
            CancellationToken cancellationToken)
        {
            return _urlCreator.GroupType switch
            {
                Grpc.GroupType.Together => await CollectIndividualReplaysAsync(_urlCreator,
                    replay => replay.PlayedTogether(_urlCreator.Identities), logger, cancellationToken),
                Grpc.GroupType.Individually =>
                    await CollectIndividualReplaysAsync(_urlCreator, _ => true, logger, cancellationToken),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private async Task<IEnumerable<Replay>> CollectIndividualReplaysAsync(ApiUrlCreator filter,
            Func<Replay, bool> condition, ILogger logger, CancellationToken cancellationToken)
        {
            var allReplays = new HashSet<Replay>();

            while (filter.Cap == 0 || allReplays.Count < filter.Cap)
            {
                var activeDownloaders = _downloaders
                    .Where(downloader => !downloader.EndReached)
                    .ToList();
                if (!activeDownloaders.Any())
                {
                    break;
                }

                foreach (var downloader in activeDownloaders)
                {
                    if (filter.Cap != 0 && allReplays.Count >= filter.Cap)
                    {
                        break;
                    }

                    var replay = await downloader.GetNextReplayAsync(cancellationToken);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return allReplays;
                    }

                    if (replay is null)
                    {
                        continue;
                    }

                    if (!condition.Invoke(replay))
                    {
                        continue;
                    }

                    if (!allReplays.Contains(replay))
                        logger.LogInformation($"Added replay {allReplays.Count + 1}: {replay.Title}");
                    allReplays.Add(replay);
                }
            }

            return allReplays;
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _disposed = true;
        }
    }
}