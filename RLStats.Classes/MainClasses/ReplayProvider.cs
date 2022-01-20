
using RLStats_Classes.MainClasses.Interfaces;
using RLStats_Classes.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace RLStats_Classes.MainClasses
{
    public class ReplayProvider : ReplayProviderBase, IReplayProvider
    {
        private bool _cancelDownload;
        private ReplayCache ReplayCache { get; set; } = new ReplayCache();

        public ReplayProvider(IAuthTokenInfo tokenInfo) : base(tokenInfo) { }

        public async Task<CollectReplaysResponse> CollectReplaysAsync(APIRequestFilter filter, bool checkCache = false)
        {
            //Don't use cache on requests with cap
            if (filter.ReplayCap > 0)
                checkCache = false;

            var response = new CollectReplaysResponse();
            InitializeNewProgress();
            ProgressState.CurrentMessage = "Downloading.";
            var sw = new Stopwatch();
            sw.Start();

            var (replays, doubleReplays) = await GetDataPack(filter, checkCache);

            //Check double replays one more time, there can be doubles between the chunks.
            var replayList = new List<Replay>(replays);
            doubleReplays += replayList.DeleteObsoleteReplays();
            replays = replayList.ToArray();

            sw.Stop();
            LastUpdateCall("Downlad finished.", replays.Length, doubleReplays);
            response.DoubleReplays = doubleReplays;
            response.Replays = replays;
            response.ElapsedMilliseconds = sw.ElapsedMilliseconds;
            GC.Collect();

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
            //init variables
            int doubleReplays = 0;
            var url = filter.GetApiUrl();
            var totalReplayCount = await Api.GetTotalReplayCountOfUrlAsync(url);
            var done = false;
            var allReplays = new List<Replay>();
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

                //delete false replays
                if (filter.CheckDate)
                    dataPack.Replays.DeleteReplaysThatAreNotInTimeRange(filter.DateRange.Item1, filter.DateRange.Item2.AddDays(1));

                doubleReplays += dataPack.Replays.DeleteObsoleteReplays();

                ProgressState.FalsePartCount += replayCountBefore - dataPack.Replays.Count;
                ProgressState.PartCount += dataPack.Replays.Count;

                //Now check if there is a cache file with the rest of the replays.But only if the checkCache flag is set
                bool gotOtherReplaysFromCache = false;
                if (checkCache)
                {
                    if (ReplayCache.HasCacheFile(filter))
                    {
                        if (ReplayCache.HasOneReplayInFile(dataPack.Replays, filter))
                        {
                            ReplayCache.AddTheOtherReplaysToTheDataPack(dataPack, filter);
                            gotOtherReplaysFromCache = true;
                        }
                    }
                }

                //add the rest to the replay batch
                allReplays.AddRange(dataPack.Replays);

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
            }
            return (allReplays.ToArray(), doubleReplays);
        }
    }
}
