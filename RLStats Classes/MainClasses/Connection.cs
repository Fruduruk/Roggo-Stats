using Newtonsoft.Json;
using RLStats_Classes.AdvancedModels;
using RLStats_Classes.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using RLStats_Classes.MainClasses.Interfaces;

namespace RLStats_Classes.MainClasses
{
    public class Connection : IDownloadProgress, IAdvancedDownloadProgress
    {

        public static event EventHandler<IDownloadProgress> DownloadProgressUpdated;
        public static event EventHandler<IAdvancedDownloadProgress> AdvancedDownloadProgressUpdated;
        public static Connection Instance { get; set; }
        public bool Initial { get; private set; } = true;
        public int ChunksToDownload { get; private set; } = 0;
        public int DownloadedChunks { get; private set; } = 0;
        public int PacksToDownload { get; private set; } = 0;
        public int DownloadedPacks { get; private set; } = 0;
        public string DownloadMessage { get; private set; } = string.Empty;
        public static double ElapsedMilliseconds { get; private set; } = 0;
        public bool Cancel { get; set; }
        private Database ReplayDatabase { get; set; }
        public IAuthTokenInfo TokenInfo { get; }
        public bool IsInitialized { get; private set; } = false;
        public static int ObsoleteReplayCount { get; private set; }
        public Stopwatch CallWatch { get; } = new Stopwatch();
        public int DownloadedReplays { get; set; } = 0;
        public int ReplaysToDownload { get; set; } = 0;


        public Connection(IAuthTokenInfo tokenInfo)
        {
            TokenInfo = tokenInfo;
            IsInitialized = true;
            ReplayDatabase = new Database();
        }

        private HttpClient GetClientWithToken()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", TokenInfo.Token);
            return client;
        }

        private async Task<HttpResponseMessage> GetAsync(string url)
        {
            HttpResponseMessage response;
            using (var client = GetClientWithToken())
            {
                response = await GetAsync(client, url);
            }
            return response;
        }

        private async Task<HttpResponseMessage> GetAsync(HttpClient client, string url)
        {
            if (CallWatch.IsRunning)
            {
                double speed = TokenInfo.GetSpeed();
                var timeToWait = (1000 / speed);
                var hasToWait = CallWatch.ElapsedMilliseconds < timeToWait;
                if (hasToWait)
                {
                    var actualTimeToWait = Math.Round(timeToWait, MidpointRounding.ToPositiveInfinity) - CallWatch.ElapsedMilliseconds;
                    Thread.Sleep((int)actualTimeToWait);
                }
                CallWatch.Stop();
            }
            CallWatch.Restart();
            var response = await client.GetAsync(url);
            return response;
        }

        public static AuthTokenInfo GetTokenInfo(string token)
        {
            var t = Task<string>.Run(async () =>
            {
                try
                {
                    HttpResponseMessage response;
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("Authorization", token);
                        response = await client.GetAsync("https://ballchasing.com/api/");
                    }
                    string content;
                    using (var reader = new StreamReader(await response.Content.ReadAsStreamAsync()))
                    {
                        content = await reader.ReadToEndAsync();
                    }
                    return content;
                }
                catch
                {
                    return string.Empty;
                }
            });
            t.Wait();
            var dataString = t.Result;
            var info = new AuthTokenInfo(token);
            if (string.IsNullOrEmpty(dataString))
            {
                info.Except = new Exception("This token is does not work.");
                return info;
            }
            dynamic jData = JsonConvert.DeserializeObject(dataString);
            if (jData != null && jData.error is null)
            {
                if (jData.chaser != null)
                    info.Chaser = jData.chaser;
                if (jData.name != null)
                    info.Name = jData.name;
                if (jData.steam_id != null)
                    info.SteamId = jData.steam_id;
                if (jData.type != null)
                    info.Type = jData.type;
                return info;
            }
            else
            {
                if (jData != null)
                    info.Except = new Exception(jData.error.ToString());
                return info;
            }
        }

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
            Cancel = false;
            GC.Collect();
            return dataPack;
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
            using var client = GetClientWithToken();
            while (!done)
            {
                var response = await GetAsync(client, url);
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
                        if (Cancel)
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
            var response = await GetAsync(url);
            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            var dataString = await reader.ReadToEndAsync();
            var currentPack = GetApiDataFromString(dataString);
            return currentPack.ReplayCount;
        }

        private void OnDownloadProgressUpdate(IDownloadProgress downloadProgress)
        {
            DownloadProgressUpdated?.Invoke(this, downloadProgress);
        }

        private void OnAdvancedDownloadProgressUpdate(IAdvancedDownloadProgress advancedDownloadProgress)
        {
            AdvancedDownloadProgressUpdated?.Invoke(this, advancedDownloadProgress);
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

        public async Task<List<AdvancedReplay>> GetAdvancedReplayInfosAsync(List<Replay> replays)
        {
            ReplayDatabase.CacheHits = 0;
            ReplayDatabase.CacheMisses = 0;
            var advancedReplays = new List<AdvancedReplay>();
            var replaysToDownload = new List<Replay>();
            var replaysToLoadFromDatabase = new List<Replay>();
            await SortReplays(replays, replaysToDownload, replaysToLoadFromDatabase);
            var replaysToReDownload = await LoadReplays(advancedReplays, replaysToLoadFromDatabase);
            replaysToDownload.AddRange(replaysToReDownload);
            await DownloadReplays(advancedReplays, replaysToDownload);
            ReplayDatabase.ReplayCache.Clear();
            DownloadMessage = $"Replays loaded: {advancedReplays.Count}\tcache hits: {ReplayDatabase.CacheHits}\tcache misses: {ReplayDatabase.CacheMisses}";
            OnAdvancedDownloadProgressUpdate(this);
            return advancedReplays;
        }

        private async Task<List<Replay>> LoadReplays(List<AdvancedReplay> advancedReplays, List<Replay> replaysToLoadFromDatabase)
        {
            ClearProgressUpdateVariables();
            ReplaysToDownload = replaysToLoadFromDatabase.Count;
            Initial = true;
            OnAdvancedDownloadProgressUpdate(this);
            Initial = false;
            if (!replaysToLoadFromDatabase.Count.Equals(0))
            {
                var count = replaysToLoadFromDatabase.Count;
                var loadedReplays = new List<AdvancedReplay>();
                var tasks = new List<Task<(bool, Replay)>>();
                await Task.Run(() =>
                {
                    foreach (var r in replaysToLoadFromDatabase)
                    {
                        try
                        {
                            var task = LoadAndAddToListAsync(loadedReplays, r, count);
                            tasks.Add(task);
                        }
                        catch
                        {
                            count--;
                        }
                    }
                    foreach (var task in tasks)
                    {
                        task.Wait();
                    }
                });
                advancedReplays.AddRange(loadedReplays);
                var notLoadedReplays = new List<Replay>();
                foreach (var task in tasks)
                {
                    if (task.Result.Item1.Equals(false))
                    {
                        notLoadedReplays.Add(task.Result.Item2);
                    }
                }
                return notLoadedReplays;
            }
            return new List<Replay>();
        }

        private async Task<(bool, Replay)> LoadAndAddToListAsync(List<AdvancedReplay> advancedReplays, Replay r, int totalCount)
        {
            if ((advancedReplays.Count % 10 == 0)|| advancedReplays.Count.Equals(totalCount))
            {
                DownloadedReplays = advancedReplays.Count;
                DownloadMessage = $"Load saved files: {advancedReplays.Count}/{totalCount}";
                OnAdvancedDownloadProgressUpdate(this);
            }

            var ar = await ReplayDatabase.LoadReplayAsync(r);
            if (ar is null) return (false, r);
            lock (advancedReplays)
            {
                advancedReplays.Add(ar);
            }
            return (true, r);
        }

        private async Task DownloadReplays(List<AdvancedReplay> advancedReplays, List<Replay> replaysToDownload)
        {
            ClearProgressUpdateVariables();
            ReplaysToDownload = replaysToDownload.Count;
            Initial = true;
            OnAdvancedDownloadProgressUpdate(this);
            Initial = false;
            var savingList = new List<AdvancedReplay>();
            using var client = GetClientWithToken();
            for (int i = 0; i < replaysToDownload.Count; i++)
            {
                var r = replaysToDownload[i];
                try
                {
                    var replay = await GetAdvancedReplayInfosAsync(client, r);
                    advancedReplays.Add(replay);
                    savingList.Add(replay);
                    if ((i % 10).Equals(0) || i.Equals(ReplaysToDownload - 1)) //Save Replays in Database every 10 Replays and at the end
                    {
                        await ReplayDatabase.SaveReplayListAsync(savingList);
                        savingList.Clear();
                    }

                    DownloadedReplays = i + 1;
                    DownloadMessage = $"Download: {i + 1}/{ReplaysToDownload}";
                    OnAdvancedDownloadProgressUpdate(this);
                }
                catch
                {
                    ReplaysToDownload--;
                }
            }
        }

        private async Task SortReplays(List<Replay> replays, List<Replay> replaysToDownload, List<Replay> replaysToLoadFromDatabase)
        {
            var count = replays.Count;
            await Task.Run(() =>
            {
                foreach (var replay in replays)
                    SortAndAddToListAsync(replaysToDownload, replaysToLoadFromDatabase, count, replay);
                while (replaysToDownload.Count + replaysToLoadFromDatabase.Count != replays.Count)
                {
                    Thread.Sleep(100);
                }
            });
        }

        private void SortAndAddToListAsync(List<Replay> replaysToDownload, List<Replay> replaysToLoadFromDatabase, int count, Replay replay)
        {
            if (!ReplayDatabase.IsReplayInDatabase(replay))
                lock (replaysToDownload)
                    replaysToDownload.Add(replay);
            else
                lock (replaysToLoadFromDatabase)
                    replaysToLoadFromDatabase.Add(replay);

            var currentCount = replaysToLoadFromDatabase.Count + replaysToDownload.Count;
            if (currentCount % 10 == 0)
            {
                //ProgressInPercent = (Convert.ToDouble(currentCount) / Convert.ToDouble(count)) * 100;
                DownloadMessage = $"Sort Replays: {currentCount}/{count}";
                OnDownloadProgressUpdate(this);
            }

            if (currentCount != count)
                return;
            //ProgressInPercent = (Convert.ToDouble(currentCount) / Convert.ToDouble(count)) * 100;
            DownloadMessage = $"Sort Replays: {currentCount}/{count}";
            OnDownloadProgressUpdate(this);
        }

        private async Task<AdvancedReplay> GetAdvancedReplayInfosAsync(HttpClient client, Replay replay)
        {
            var url = APIRequestBuilder.GetSpecificReplayUrl(replay.Id);
            var response = await GetAsync(client, url);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Couldn't load Advanced Replay: {response.ReasonPhrase}");
            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            var dataString = await reader.ReadToEndAsync();
            return AdvancedReplayAssembler.GetAdvancedReplayFromString(dataString);
        }
    }
}
