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
    public class ReplayCollector
    {
        private readonly ApiUrlCreator _urlCreator;
        private readonly IBallchasingApi _api;
        private IReplayCache _replayCache;
        private readonly List<SimpleReplayDownloader> _downloaders;

        public ReplayCollector(ApiUrlCreator urlCreator, IBallchasingApi api, IReplayCache replayCache, ILogger logger)
        {
            _urlCreator = urlCreator;
            _api = api;
            _replayCache = replayCache;
            _downloaders = urlCreator.Urls.Select(url =>
                new SimpleReplayDownloader(_api, url, logger)).ToList();
        }

        public async Task<CollectReplaysResponse> CollectReplaysAsync(ILogger logger,
            CancellationToken cancellationToken)
        {
            logger.LogInformation("Started collecting replays.");

            var response = new CollectReplaysResponse();
            var sw = new Stopwatch();
            sw.Start();

            var replays = await GetReplays(logger, cancellationToken);

            sw.Stop();

            response.ElapsedMilliseconds = sw.ElapsedMilliseconds;

            if (cancellationToken.IsCancellationRequested)
            {
                logger.LogInformation("Cancelled collecting replays.");
                return response;
            }

            response.Replays = replays.ToList();

            logger.LogInformation("Finished collecting replays.");
            logger.LogDebugObject(new
            {
                ReplayCount = response.Replays.Count(),
                sw.ElapsedMilliseconds
            });
            logger.LogDebugObject(_api);

            return response;
        }

        private async Task<IEnumerable<Replay>> GetReplays(ILogger logger,
            CancellationToken cancellationToken)
        {
            return _urlCreator.GroupType switch
            {
                Grpc.GroupType.Together =>
                    await CollectIndividualReplaysAsync(_urlCreator,
                        replay => replay.PlayedTogether(_urlCreator.Identities),
                        logger, cancellationToken),
                Grpc.GroupType.Individually =>
                    await CollectIndividualReplaysAsync(_urlCreator,
                        replay => replay.ContainsAtLeastOneIdentity(_urlCreator.Identities),
                        logger, cancellationToken),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private async Task<IEnumerable<Replay>> CollectIndividualReplaysAsync(ApiUrlCreator filter,
            Func<Replay, bool> condition, ILogger logger, CancellationToken cancellationToken)
        {
            var firstReplaysDone = false;
            var cachedReplays = await _replayCache.LoadCachedReplays(filter);
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

                // Load the first [downloader count] replays and check if they are in cache.
                // We have to check every downloader unfortunately.
                // If true then the cache can be safely loaded.
                // If false then the cache should be overwritten.
                if (!firstReplaysDone && cachedReplays is not null && allReplays.Count > 0)
                {
                    if (allReplays.All(replay => cachedReplays.Contains(replay)))
                    {
                        logger.LogInformation($"Found {cachedReplays.Count} in cache.");
                        return cachedReplays;
                    }
                }

                firstReplaysDone = true;
            }

            await _replayCache.WriteReplayCache(filter, allReplays);
            return allReplays;
        }
    }
}