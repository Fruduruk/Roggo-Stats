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

namespace RLStats_Classes.MainClasses
{
    public class Connection
    {

        public static event EventHandler<double> ProgressUpdated;
        public static event EventHandler<double> AdvancedProgressUpdated;
        public static event EventHandler<string> ProgressChanged;
        public static event EventHandler<int> DownloadStarted;
        public static event EventHandler<string> AdvancedProgressChanged;
        public static Connection Instance { get; set; }
        public static double ElapsedMilliseconds { get; private set; } = 0;
        public bool Cancel { get; set; }
        private Database ReplayDatabase { get; set; }
        public AuthTokenInfo TokenInfo { get; }
        public bool IsInitialized { get; private set; } = false;
        public static int ObsoleteReplayCount { get; private set; }

        public Connection(AuthTokenInfo tokenInfo)
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

        public static AuthTokenInfo GetTokenInfo(string token)
        {
            using var client = new HttpClient();
            var t = Task<string>.Run(async () =>
            {
                try
                {
                    client.DefaultRequestHeaders.Add("Authorization", token);
                    var response = await client.GetAsync("https://ballchasing.com/api/");
                    using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
                    return await reader.ReadToEndAsync();
                }
                catch
                {
                    return string.Empty;
                }
            });
            t.Wait();
            string dataString = t.Result;
            AuthTokenInfo info = new AuthTokenInfo(token);
            if (string.IsNullOrEmpty(dataString))
            {
                info.Except = new Exception("This token is does not work.");
                return info;
            }
            dynamic jData = JsonConvert.DeserializeObject(dataString);
            if (jData.error is null)
            {
                if (jData.chaser != null)
                    info.Chaser = jData.chaser;
                if (jData.name != null)
                    info.Name = jData.name;
                if (jData.steam_id != null)
                    info.SteamID = jData.steam_id;
                if (jData.type != null)
                    info.Type = jData.type;
                return info;
            }
            else
            {
                info.Except = new Exception(jData.error.ToString());
                return info;
            }
        }
        public async Task<ApiDataPack> CollectReplaysAsync(APIRequestFilter builder)
        {
            OnProgressChange("Download started...");
            OnProgressUpdate(0);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            ApiDataPack dataPack = await GetDataPack(builder);
            sw.Stop();
            OnProgressChange("Downoald done...");
            OnProgressChange("Delete obsolete replays");
            ObsoleteReplayCount = dataPack.DeleteObsoleteReplays();
            OnProgressChange("Delete replays without specific names");
            ElapsedMilliseconds = sw.ElapsedMilliseconds;
            Cancel = false;
            GC.Collect();
            return dataPack;
        }

        private async Task<ApiDataPack> GetDataPack(APIRequestFilter urlBuilder)
        {
            int replayCount = await GetReplayCountOfUrlAsync(urlBuilder.GetApiUrl());
            double steps = Convert.ToDouble(replayCount) / 50;
            steps = Math.Round(steps, MidpointRounding.ToPositiveInfinity);
            double stepsDone = 0;
            ShowUpdate(steps, stepsDone, 0);
            OnDownloadStart(replayCount);
            var url = urlBuilder.GetApiUrl();
            bool done = false;
            ApiDataPack allData = new ApiDataPack
            {
                Replays = new List<Replay>(),
                ReplayCount = replayCount
            };
            using var client = GetClientWithToken();
            while (!done)
            {
                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
                    string dataString = await reader.ReadToEndAsync();
                    Thread.Sleep(1000 / TokenInfo.GetSpeed());
                    var currentPack = GetApiDataFromString(dataString);
                    if (currentPack.Success)
                    {
                        allData.Success = true;
                        if (urlBuilder.CheckForDate)
                            currentPack.DeleteReplaysThatAreNotInTimeRange(urlBuilder.StartDate, urlBuilder.EndDate);
                        allData.Replays.AddRange(currentPack.Replays);
                        stepsDone++;
                        ShowUpdate(steps, stepsDone, currentPack.Replays.Count);
                        if (Cancel)
                            break;
                        if (currentPack.Next != null)
                            url = currentPack.Next;
                        else
                            done = true;
                    }
                    else
                        done = true;
                }
                else
                {
                    Thread.Sleep(5000);
                }
            }
            if (!allData.Success)
            {
                allData.Ex = new Exception("no Data could be collected");
                allData.ReplayCount = 0;
            }
            return allData;
        }

        private void ShowUpdate(double steps, double stepsDone, int count)
        {
            OnProgressUpdate(stepsDone / steps * 100);
            OnProgressChange($"Progress: {stepsDone}/{steps}" + ((count != 0) ? "\t+" + count : ""));
        }

        private async Task<int> GetReplayCountOfUrlAsync(string url)
        {
            using var client = GetClientWithToken();
            HttpResponseMessage response = await client.GetAsync(url);
            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            string dataString = await reader.ReadToEndAsync();
            var currentPack = GetApiDataFromString(dataString);
            return currentPack.ReplayCount;
        }

        private void OnProgressUpdate(double value)
        {
            ProgressUpdated?.Invoke(this, value);
        }
        private void OnAdvancedProgressUpdate(double value)
        {
            AdvancedProgressUpdated?.Invoke(this, value);
        }
        private void OnProgressChange(string value)
        {
            ProgressChanged?.Invoke(this, value);
        }
        private void OnAdvancedProgressChange(string value)
        {
            AdvancedProgressChanged?.Invoke(this, value);
        }
        private void OnDownloadStart(int value)
        {
            DownloadStarted?.Invoke(this, value);
        }

        private ApiDataPack GetApiDataFromString(string dataString)
        {
            dynamic jData = JsonConvert.DeserializeObject(dataString);
            var replays = new List<Replay>();
            try
            {
                foreach (var r in jData.list)
                {
                    var replay = new Replay();
                    replay.ID = r.id;
                    replay.RocketLeagueID = r.rocket_league_id;
                    if (r.season_type is null)
                        replay.SeasonType = "before Free2Play";
                    else
                        replay.SeasonType = r.season_type;
                    replay.Visibility = r.visibility;
                    replay.Link = r.link;
                    replay.Title = r.replay_title;
                    replay.Playlist = r.playlist_id;
                    replay.Season = r.season;
                    replay.Date = r.date;
                    replay.Uploader = r.uploader.name;
                    replay.Blue = GetTeam(r.blue);
                    replay.Orange = GetTeam(r.orange);
                    replays.Add(replay);
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
            var advancedReplays = new List<AdvancedReplay>();
            var replaysToDownload = new List<Replay>();
            var replaysToLoadFromDatabase = new List<Replay>();
            await SortReplays(replays, replaysToDownload, replaysToLoadFromDatabase);
            var savingList = await DownloadReplays(advancedReplays, replaysToDownload);
            await LoadReplays(advancedReplays, replaysToLoadFromDatabase);
            ReplayDatabase.SaveReplayListAsync(savingList);
            OnAdvancedProgressChange($"Replays loaded: {advancedReplays.Count}");
            return advancedReplays;
        }

        private async Task LoadReplays(List<AdvancedReplay> advancedReplays, List<Replay> replaysToLoadFromDatabase)
        {
            if (!replaysToLoadFromDatabase.Count.Equals(0))
            {
                var count = replaysToLoadFromDatabase.Count;
                var loadedReplays = new List<AdvancedReplay>();
                await Task.Run(() =>
                {
                    for (int i = 0; i < replaysToLoadFromDatabase.Count; i++)
                    {
                        Replay r = replaysToLoadFromDatabase[i];
                        try
                        {
                            LoadAndAddToListAsync(loadedReplays, r, count);
                        }
                        catch
                        {
                            count--;
                        }
                    }
                    while (loadedReplays.Count != count)
                    {
                        Thread.Sleep(100);
                    }
                });
                advancedReplays.AddRange(loadedReplays);
            }
        }

        private async void LoadAndAddToListAsync(List<AdvancedReplay> advancedReplays, Replay r, int totalCount)
        {
            var ar = await ReplayDatabase.LoadReplayAsync(r);
            lock (advancedReplays)
            {
                advancedReplays.Add(ar);
            }

            if (advancedReplays.Count % 10 == 0)
            {
                OnAdvancedProgressUpdate((Convert.ToDouble(advancedReplays.Count) / Convert.ToDouble(totalCount)) * 100);
                OnAdvancedProgressChange($"Load saved files: {advancedReplays.Count}/{totalCount}");
            }
            if (advancedReplays.Count == totalCount)
            {
                OnAdvancedProgressUpdate((Convert.ToDouble(advancedReplays.Count) / Convert.ToDouble(totalCount)) * 100);
                OnAdvancedProgressChange($"Load saved files: {advancedReplays.Count}/{totalCount}");
            }
        }

        private async Task<List<AdvancedReplay>> DownloadReplays(List<AdvancedReplay> advancedReplays, List<Replay> replaysToDownload)
        {
            var count = replaysToDownload.Count;
            var savingList = new List<AdvancedReplay>();
            using var client = GetClientWithToken();
            for (int i = 0; i < replaysToDownload.Count; i++)
            {
                Replay r = replaysToDownload[i];
                try
                {
                    var replay = await GetAdvancedReplayInfosAsync(client, r);
                    Thread.Sleep(1000 / TokenInfo.GetSpeed());
                    advancedReplays.Add(replay);
                    savingList.Add(replay);
                    OnAdvancedProgressUpdate((Convert.ToDouble(i + 1) / Convert.ToDouble(count)) * 100);
                    OnAdvancedProgressChange($"Download: {i + 1}/{count}");
                }
                catch
                {
                    count--;
                }
            }
            return savingList;
        }

        private async Task SortReplays(List<Replay> replays, List<Replay> replaysToDownload, List<Replay> replaysToLoadFromDatabase)
        {
            var count = replays.Count;
            await Task.Run(() =>
            {
                for (int i = 0; i < replays.Count; i++)
                {
                    var replay = replays[i];
                    SortAndAddToListAsync(replaysToDownload, replaysToLoadFromDatabase, count, replay);
                }
                while (replaysToDownload.Count + replaysToLoadFromDatabase.Count != replays.Count)
                {
                    Thread.Sleep(100);
                }
            });
        }

        private async void SortAndAddToListAsync(List<Replay> replaysToDownload, List<Replay> replaysToLoadFromDatabase, int count, Replay replay)
        {
            if (await ReplayDatabase.GetReplayPath(replay) is null)
                lock (replaysToDownload)
                    replaysToDownload.Add(replay);
            else
                lock (replaysToLoadFromDatabase)
                    replaysToLoadFromDatabase.Add(replay);

            var currentCount = replaysToLoadFromDatabase.Count + replaysToDownload.Count;
            if (currentCount % 10 == 0)
            {
                OnAdvancedProgressUpdate((Convert.ToDouble(currentCount) / Convert.ToDouble(count)) * 100);
                OnAdvancedProgressChange($"Sort Replays: {currentCount}/{count}");
            }
            if (currentCount == count)
            {
                OnAdvancedProgressUpdate((Convert.ToDouble(currentCount) / Convert.ToDouble(count)) * 100);
                OnAdvancedProgressChange($"Sort Replays: {currentCount}/{count}");
            }
        }

        private async Task<AdvancedReplay> GetAdvancedReplayInfosAsync(HttpClient client, Replay replay)
        {
            var url = APIRequestFilter.GetSpecificReplayUrl(replay.ID);
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
                string dataString = await reader.ReadToEndAsync();
                return GetAdvancedReplayFromString(dataString);
            }
            else
            {
                throw new Exception($"Couldn't load Advanced Replay: {response.ReasonPhrase}");
            }
        }

        private AdvancedReplay GetAdvancedReplayFromString(string dataString)
        {
            dynamic jData = JsonConvert.DeserializeObject(dataString);
            var ara = new AdvancedReplayAssembler(jData);
            AdvancedReplay aReplay = ara.Assemble();
            return aReplay;
        }

        private static Team GetTeam(dynamic r)
        {
            Team t = new Team();
            if (r.goals != null)
                t.Goals = r.goals;
            if (r.players != null)
                foreach (var p in r.players)
                {
                    var player = new Player();
                    player.Name = p.name;
                    if (p.mvp != null)
                        player.MVP = p.mvp;
                    else
                        player.MVP = false;
                    if (p.score != null)
                        player.Score = p.score;
                    else
                        player.Score = 0;
                    player.StartTime = p.start_time;
                    player.EndTime = p.end_time;
                    t.Players.Add(player);
                }
            return t;
        }
    }
}
