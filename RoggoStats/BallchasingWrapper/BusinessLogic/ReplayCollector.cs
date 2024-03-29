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

                // Download replays as usual.
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
                    {
                        logger.LogInformation($"Added replay {allReplays.Count + 1}: {replay.Title}");
                        downloader.IncrementReplaysProvided();
                    }

                    allReplays.Add(replay);
                }

                // Load the first [downloader count] replays and check if they are in cache.
                // We have to check every downloader unfortunately.
                // If true then the cache can be safely loaded.
                // If false then the cache should be overwritten.
                if (cachedReplays is not null && allReplays.Count > 0)
                {
                    // If no new Replay was uploaded there is no need to proceed downloading
                    // and the cache can be loaded in its entirety.
                    if (!firstReplaysDone && allReplays.All(replay => cachedReplays.Contains(replay)))
                    {
                        logger.LogInformation($"Found {cachedReplays.Count} in cache.");
                        return cachedReplays;
                    }

                    // If there is at least one replay from every downloader in cache
                    // the rest of the cache can be loaded and updated.
                    // Only check active downloaders because the others won't have any new replays
                    // and we are checking if the download should continue.
                    if (activeDownloaders.Where(downloader => downloader.ReplaysProvided.Any()).All(
                            downloader => downloader.ReplaysProvided.Any(replay => cachedReplays.Contains(replay))))
                    {
                        FillUpWithCachedReplays(allReplays, cachedReplays, filter.Cap);
                        await _replayCache.WriteReplayCache(filter, allReplays);
                        return allReplays;
                    }
                }

                firstReplaysDone = true;
            }

            await _replayCache.WriteReplayCache(filter, allReplays);
            return allReplays;
        }


        private static void FillUpWithCachedReplays(HashSet<Replay> allReplays, IEnumerable<Replay> cachedReplays,
            int filterCap)
        {
            foreach (var replay in cachedReplays)
            {
                if (filterCap == 0 || allReplays.Count < filterCap)
                {
                    allReplays.Add(replay);
                }
            }
        }
    }
}