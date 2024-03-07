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
        private bool _cancelDownload;
        private readonly IBallchasingApi _api;
        private IReplayCache _replayCache;
        private ILogger _logger;

        public ReplayCollector(IBallchasingApi api, IReplayCache replayCache, ILogger logger)
        {
            _api = api;
            _replayCache = replayCache;
            _logger = logger;
        }

        public async Task<CollectReplaysResponse> CollectReplaysAsync(ApiUrlCreator filter,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Started collecting replays.");

            var response = new CollectReplaysResponse();
            var sw = new Stopwatch();
            sw.Start();

            var replays = await GetReplays(filter, cancellationToken);

            sw.Stop();
            _logger.LogInformation("Finished collecting replays.");

            response.Replays = replays;
            response.ElapsedMilliseconds = sw.ElapsedMilliseconds;

            LogDebugObject(new
            {
                ReplayCount = replays.Count,
                sw.ElapsedMilliseconds
            });

            _cancelDownload = false;
            return response;
        }

        public void CancelDownload() => _cancelDownload = true;

        private async Task<List<Replay>> GetReplays(ApiUrlCreator filter,
            CancellationToken cancellationToken)
        {
            return filter.GroupType switch
            {
                Grpc.GroupType.Together => await CollectIndividualReplaysAsync(filter, cancellationToken),
                Grpc.GroupType.Individually => await CollectIndividualReplaysAsync(filter, cancellationToken),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private async Task<List<Replay>> CollectIndividualReplaysAsync(ApiUrlCreator filter,
            CancellationToken cancellationToken)
        {
            var allReplays = new HashSet<Replay>();
            var downloaders = filter.Urls.Select(url =>
                new SimpleReplayDownloader(_api, url, cancellationToken)).ToList();

            while (!downloaders.All(downloader => downloader.EndReached) && (filter.Cap == 0|| allReplays.Count < filter.Cap))
            {
                foreach (var downloader in downloaders)
                {
                    if (filter.Cap == 0|| allReplays.Count < filter.Cap)
                    {
                        var replay = await downloader.DownloadNextReplayAsync();
                        if (replay is null)
                        {
                            continue;
                        }
                        allReplays.Add(replay);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return allReplays.ToList();
        }

        private async Task<List<Replay>> CollectTeamReplaysAsync(ApiUrlCreator filter,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        

        private void LogDebugObject(object obj)
        {
            _logger.LogDebug(JsonConvert.SerializeObject(obj, Formatting.Indented));
        }
    }
}