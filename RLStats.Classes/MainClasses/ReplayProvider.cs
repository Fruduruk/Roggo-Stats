using RLStats_Classes.MainClasses.Interfaces;
using RLStats_Classes.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RLStats_Classes.MainClasses
{
    public class ReplayProvider : ReplayProviderBase, IReplayProvider
    {
        private bool _cancelDownload;

        public ReplayProvider(IAuthTokenInfo tokenInfo) : base(tokenInfo) { }

        public async Task<CollectReplaysResponse> CollectReplaysAsync(APIRequestFilter filter)
        {
            var response = new CollectReplaysResponse();
            InitializeNewProgress();
            ProgressState.CurrentMessage = "Downloading.";
            var sw = new Stopwatch();
            sw.Start();
            var (replays, doubleReplays) = await GetDataPack(filter);
            sw.Stop();
            ProgressState.CurrentMessage = "Downlad finished.";
            response.DoubleReplays = doubleReplays;
            response.Replays = replays;
            response.ElapsedMilliseconds = sw.ElapsedMilliseconds;
            _cancelDownload = false;
            GC.Collect();
            return response;
        }

        public void CancelDownload() => _cancelDownload = true;

        private async Task<(Replay[], int doubleReplays)> GetDataPack(APIRequestFilter filter)
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

            //var steps = Convert.ToDouble(totalReplayCount) / 50;
            //steps = Math.Round(steps, MidpointRounding.ToPositiveInfinity);

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
                if (_cancelDownload)
                {
                    ProgressState.CurrentMessage = "Cancel requested.\nDownload stopped.";
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
