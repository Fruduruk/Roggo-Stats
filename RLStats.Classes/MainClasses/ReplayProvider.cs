using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using RLStats_Classes.MainClasses.CacheHandlers;
using RLStats_Classes.MainClasses.Interfaces;
using RLStats_Classes.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RLStats_Classes.MainClasses
{
    public class ReplayProvider : ReplayProviderBase, IReplayProvider
    {
        private bool _cancelDownload;
        private ReplayCache ReplayCache { get; set; } = new ReplayCache();

        public ReplayProvider(IAuthTokenInfo tokenInfo, ILogger logger) : base(tokenInfo, logger) { }

        public async Task<CollectReplaysResponse> CollectReplaysAsync(APIRequestFilter filter, bool checkCache = false)
        {
            Logger.LogInformation("Started collecting replays.");
            //Don't use cache on requests with cap
            if (filter.ReplayCap > 0)
                checkCache = false;

            var response = new CollectReplaysResponse();
            InitializeNewProgress();
            ProgressState.CurrentMessage = "Downloading.";
            var sw = new Stopwatch();
            sw.Start();

            var (replays, doubleReplays) = await GetDataPack(filter, checkCache);

            sw.Stop();
            LastUpdateCall("Downlad finished.", replays.Length, doubleReplays);
            Logger.LogInformation("Finished collecting replays.");
            
            response.DoubleReplays = doubleReplays;
            response.Replays = replays;
            response.ElapsedMilliseconds = sw.ElapsedMilliseconds;
            GC.Collect();

            LogDebugObject(new
            {
                ReplayCount = replays.Length,
                FalsePartCount = doubleReplays,
                sw.ElapsedMilliseconds
            });

            //Don't use cache on requests with cap
            if (filter.ReplayCap == 0)
                if (!_cancelDownload)
                    ReplayCache.StoreReplaysInCache(response, filter);

            _cancelDownload = false;
            return response;
        }

        public async Task<CollectReplaysResponse> CollectReplaysAsync(APIRequestFilter filter, CancellationToken cancellationToken, bool checkCache = false)
        {
            Api.StoppingToken = cancellationToken;
            return await CollectReplaysAsync(filter, checkCache);
        }

        public void CancelDownload() => _cancelDownload = true;

        private async Task<(Replay[], int doubleReplays)> GetDataPack(APIRequestFilter filter, bool checkCache)
        {
            LogDebugObject(new
            {
                PackUrl = filter.GetApiUrl(),
                filter.Names
            });
            //init variables
            var url = filter.GetApiUrl();
            var totalReplayCount = await Api.GetTotalReplayCountOfUrlAsync(url);
            if (totalReplayCount >= 10_000)
                throw new Exception($"Total replays to download exceeds the download limit of 10000. Download canceled.");
            var done = false;
            var doubleReplays = 0;
            var allReplays = new HashSet<Replay>();
            ProgressState.TotalCount = totalReplayCount;

            //check if there are replays to download
            if (totalReplayCount.Equals(0))
                return (Array.Empty<Replay>(), 0);


            while (!done)
            {
                //download new pack
                var dataPack = await Api.GetApiDataPack(url);
                if (!dataPack.Success)
                    return (Array.Empty<Replay>(), 0);

                var replayCountBefore = dataPack.Replays.Count;

                //Delete all obsolete replays in this chunk
                var hashedReplayChunk = new HashSet<Replay>(dataPack.Replays);
                Logger.LogDebug($"Removed {dataPack.Replays.Count - hashedReplayChunk.Count} duplicates");

                //delete false replays
                if (filter.CheckDate)
                    hashedReplayChunk.DeleteReplaysThatAreNotInTimeRange(filter.DateRange.Item1, filter.DateRange.Item2.AddDays(1));

                if (hashedReplayChunk.Count.Equals(0)) //If the query got out of range there is no need to seek farther; maybe there is; dont know yet.
                    done = true;

                hashedReplayChunk.DeleteReplaysThatDoNotHaveTheActualNamesInIt(filter.Names);

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
                if (!filter.ReplayCap.Equals(0))
                    if (allReplays.Count >= filter.ReplayCap)
                    {
                        allReplays.TrimReplaysToCap(filter.ReplayCap);
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
            return (allReplays.ToArray(), doubleReplays);
        }

        private void LogDebugObject(object obj)
        {
            Logger.LogDebug(JsonConvert.SerializeObject(obj, Formatting.Indented));
        }
    }
}
