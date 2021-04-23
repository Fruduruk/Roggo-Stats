using Newtonsoft.Json;
using RLStats_Classes.MainClasses.Interfaces;
using RLStats_Classes.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RLStats_Classes.MainClasses
{
    public class ReplayProvider : ReplayProviderBase, IReplayProvider
    {

        public static event EventHandler<IDownloadProgress> DownloadProgressUpdated;
        public bool Initial { get; private set; } = true;
        public int ChunksToDownload { get; private set; } = 0;
        public int DownloadedChunks { get; private set; } = 0;
        public int PacksToDownload { get; private set; } = 0;
        public int DownloadedPacks { get; private set; } = 0;
        public string DownloadMessage { get; private set; } = string.Empty;
        public static double ElapsedMilliseconds { get; private set; } = 0;
        public static int ObsoleteReplayCount { get; private set; }
        public int DownloadedReplays { get; set; } = 0;
        public int ReplaysToDownload { get; set; } = 0;

        private bool _cancelDownload;


        public ReplayProvider(IAuthTokenInfo tokenInfo) : base(tokenInfo) { }
        
        public async Task<ApiDataPack> CollectReplaysAsync(APIRequestFilter filter)
        {
            ClearProgressUpdateVariables();
            Initial = true;
            DownloadMessage = "Download started...";
            OnDownloadProgressUpdate(this);
            var sw = new Stopwatch();
            sw.Start();
            var dataPack = await GetDataPack(filter);
            sw.Stop();
            ElapsedMilliseconds = sw.ElapsedMilliseconds;
            _cancelDownload = false;
            GC.Collect();
            return dataPack;
        }

        public void CancelDownload()
        {
            _cancelDownload = true;
        }

        private void ClearProgressUpdateVariables()
        {
            PacksToDownload = 0;
            ChunksToDownload = 0;
            DownloadedPacks = 0;
            DownloadedChunks = 0;
            ReplaysToDownload = 0;
            DownloadedReplays = 0;
            DownloadMessage = string.Empty;
        }

        private async Task<ApiDataPack> GetDataPack(APIRequestFilter filter)
        {
            PacksToDownload = await GetReplayCountOfUrlAsync(filter.GetApiUrl());
            ReplaysToDownload = PacksToDownload;
            if (PacksToDownload.Equals(0))
                return new ApiDataPack()
                {
                    Success = false,
                    Ex = new Exception("No Replays found."),
                    ReceivedString = string.Empty,
                    ReplayCount = 0,
                    Replays = new List<Replay>()
                };
            var steps = Convert.ToDouble(PacksToDownload) / 50;
            steps = Math.Round(steps, MidpointRounding.ToPositiveInfinity);
            ChunksToDownload = Convert.ToInt32(steps);
            DownloadedChunks = 0;
            DownloadMessage = $"Progress: {DownloadedChunks}/{ChunksToDownload}";
            OnDownloadProgressUpdate(this);
            Initial = false;
            var url = filter.GetApiUrl();
            var done = false;
            var allData = new ApiDataPack
            {
                Replays = new List<Replay>(),
                ReplayCount = PacksToDownload
            };
            while (!done)
            {
                var response = await Api.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
                    var dataString = await reader.ReadToEndAsync();
                    var currentPack = GetApiDataFromString(dataString);
                    if (currentPack.Success)
                    {
                        PacksToDownload = currentPack.Replays.Count;
                        allData.Success = true;
                        DownloadedReplays += currentPack.Replays.Count;
                        if (filter.CheckDate)
                            currentPack.DeleteReplaysThatAreNotInTimeRange(filter.DateRange.Item1, filter.DateRange.Item2.AddDays(1));
                        ObsoleteReplayCount += currentPack.DeleteObsoleteReplays();
                        allData.Replays.AddRange(currentPack.Replays);
                        DownloadedChunks++;
                        DownloadMessage = $"Progress: {DownloadedChunks}/{ChunksToDownload}" +
                                          (currentPack.Replays.Count != 0 ? "\t+" + currentPack.Replays.Count : "");
                        DownloadedPacks = currentPack.Replays.Count;
                        if (!filter.ReplayCap.Equals(0))
                            if (DownloadedPacks >= filter.ReplayCap)
                                done = true;
                        if (DownloadedReplays >= ReplaysToDownload)
                            done = true;
                        OnDownloadProgressUpdate(this);
                        if (_cancelDownload)
                            break;
                        if (currentPack.Next != null)
                            url = currentPack.Next;
                        else
                            done = true;
                    }
                    else
                    {
                        DownloadedPacks = 0;
                        DownloadMessage = "Fetching replay pack failed";
                        OnDownloadProgressUpdate(this);
                        done = true;
                    }
                }
                else
                {
                    Thread.Sleep(5000);
                }
            }

            if (allData.Success)
            {
                if (!filter.ReplayCap.Equals(0))
                    allData.TrimReplaysToCap(filter.ReplayCap);
                return allData;
            }
            allData.Ex = new Exception("no Data could be collected");
            allData.ReplayCount = 0;
            return allData;
        }

        private async Task<int> GetReplayCountOfUrlAsync(string url)
        {
            var response = await Api.GetAsync(url);
            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            var dataString = await reader.ReadToEndAsync();
            var currentPack = GetApiDataFromString(dataString);
            return currentPack.ReplayCount;
        }

        private void OnDownloadProgressUpdate(IDownloadProgress downloadProgress)
        {
            DownloadProgressUpdated?.Invoke(this, downloadProgress);
        }

        private static ApiDataPack GetApiDataFromString(string dataString)
        {
            dynamic jData = JsonConvert.DeserializeObject(dataString);
            var replays = new List<Replay>();
            try
            {
                if (jData != null)
                    foreach (var r in jData.list)
                    {
                        replays.Add(new ReplayAssembler(r).Assemble());
                    }

                if (replays.Count == 0)
                    throw new Exception("No replay found");
            }
            catch (Exception e)
            {
                return new ApiDataPack()
                {
                    Success = false,
                    ReceivedString = dataString,
                    Ex = e
                };
            }
            return new ApiDataPack()
            {
                Replays = replays,
                ReplayCount = jData.count,
                Next = jData.next,
                Success = true,
            };
        }

    }
}
