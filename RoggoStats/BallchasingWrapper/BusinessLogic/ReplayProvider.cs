using System.Diagnostics;
using BallchasingWrapper.Extensions;
using BallchasingWrapper.Interfaces;
using BallchasingWrapper.Models;
using BallchasingWrapper.Models.ReplayModels;

namespace BallchasingWrapper.BusinessLogic
{
    public class ReplayProvider : ReplayProviderBase, IReplayProvider
    {
        private bool _cancelDownload;
        private IReplayCache ReplayCache { get; }

        public ReplayProvider(BallchasingApi api, IReplayCache replayCache, ILogger logger) : base(api, logger)
        {
            ReplayCache = replayCache;
        }

        public async Task<CollectReplaysResponse> CollectReplaysAsync(ApiUrlCreator filter, bool checkCache = false)
        {
            Logger.LogInformation("Started collecting replays.");

            var response = new CollectReplaysResponse();
            InitializeNewProgress();
            ProgressState.CurrentMessage = "Downloading.";
            var sw = new Stopwatch();
            sw.Start();

            var replays = await GetReplays(filter, checkCache);

            sw.Stop();
            LastUpdateCall("Download finished.", replays.Length);
            Logger.LogInformation("Finished collecting replays.");

            response.Replays = replays;
            response.ElapsedMilliseconds = sw.ElapsedMilliseconds;
            GC.Collect();

            LogDebugObject(new
            {
                ReplayCount = replays.Length,
                sw.ElapsedMilliseconds
            });

            _cancelDownload = false;
            return response;
        }

        public async Task<CollectReplaysResponse> CollectReplaysAsync(ApiUrlCreator filter, CancellationToken cancellationToken, bool checkCache = false)
        {
            Api.StoppingToken = cancellationToken;
            return await CollectReplaysAsync(filter, checkCache);
        }

        public void CancelDownload() => _cancelDownload = true;

        private async Task<Replay[]> GetReplays(ApiUrlCreator filter, bool checkCache)
        {
            LogDebugObject(new
            {
                PackUrl = filter.Urls,
            });
            //init variables
            var url = filter.Urls.First();
            var firstPack = await Api.GetApiDataPack(url);
            var firstIteration = true;
            if (!firstPack.Success)
                return Array.Empty<Replay>();

            var totalReplayCount = firstPack.ReplayCount;
            if (totalReplayCount >= 10_000)
                throw new Exception($"Total replays to download exceeds the download limit of 10000. Download canceled.");
            var done = false;
            var doubleReplays = 0;
            var allReplays = new HashSet<Replay>();
            ProgressState.TotalCount = totalReplayCount;

            //check if there are replays to download
            if (totalReplayCount.Equals(0))
                return Array.Empty<Replay>();


            while (!done)
            {
                //download new pack
                var dataPack = firstIteration ? firstPack : await Api.GetApiDataPack(url);
                firstIteration = false;
                if (!dataPack.Success)
                    return Array.Empty<Replay>();

                var replayCountBefore = dataPack.Replays.Count;

                //Delete all obsolete replays in this chunk
                var hashedReplayChunk = new HashSet<Replay>(dataPack.Replays);
                Logger.LogDebug($"Removed {dataPack.Replays.Count - hashedReplayChunk.Count} duplicates");

                //delete false replays
                //if (filter.)
                //    hashedReplayChunk.DeleteReplaysThatAreNotInTimeRange(filter.DateRange.Item1, filter.DateRange.Item2);

                if (hashedReplayChunk.Count.Equals(0)) //If the query got out of range there is no need to seek farther; maybe there is; dont know yet.
                    done = true;

                //hashedReplayChunk.DeleteReplaysThatDoNotHaveTheActualNamesInIt(filter.Names);

                doubleReplays += replayCountBefore - hashedReplayChunk.Count;
                ProgressState.FalsePartCount += replayCountBefore - hashedReplayChunk.Count;
                ProgressState.PartCount += hashedReplayChunk.Count;

                //Now check if there is a cache file with the rest of the replays.But only if the checkCache flag is set
                bool gotOtherReplaysFromCache = false;
                if (checkCache)
                {
                    if (ReplayCache.HasCacheFile(filter))
                    {
                        if (ReplayCache.HasOneReplayInFile(hashedReplayChunk, filter))
                        {
                            ReplayCache.AddTheOtherReplaysToTheDataPack(hashedReplayChunk, filter);
                            gotOtherReplaysFromCache = true;
                        }
                    }
                }

                //add the rest to the replay batch
                allReplays.UnionWith(hashedReplayChunk);

                //check if replay count exceeds replay cap
                if (!filter.Cap.Equals(0))
                    if (allReplays.Count >= filter.Cap)
                    {
                        allReplays.TrimReplaysToCap(filter.Cap);
                        done = true;
                    }

                //check if cancel is requested
                if (_cancelDownload || Api.StoppingToken.IsCancellationRequested)
                {
                    ProgressState.CurrentMessage = "Cancel requested.\nDownload stopped.";
                    break;
                }

                if (gotOtherReplaysFromCache)
                {
                    ProgressState.CurrentMessage = "Found replays in Cache.\nDownload stopped.";
                    break;
                }

                //check if there are more replays to download
                if (dataPack.Next != null)
                    url = dataPack.Next;
                else
                    done = true;

                Logger.LogDebug($"Current replay count: {allReplays.Count}");
            }
            return allReplays.ToArray();
        }

        private void LogDebugObject(object obj)
        {
            Logger.LogDebug(JsonConvert.SerializeObject(obj, Formatting.Indented));
        }
    }
}
