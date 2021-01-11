using Newtonsoft.Json;
using RocketLeagueStats.AdvancedModels;
using RocketLeagueStats.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace RocketLeagueStats
{
    public class Connection
    {

        private static Connection instance;
        public static event EventHandler<double> ProgressUpdated;
        public static event EventHandler<double> AdvancedProgressUpdated;
        public static event EventHandler<string> ProgressChanged;
        public static event EventHandler<int> DownloadStarted;
        public static event EventHandler<string> AdvancedProgressChanged;
        public static Connection Instance
        {
            get
            {
                return instance;
            }
            set
            {
                instance = value;
            }
        }
        private HttpClient Client { get; set; } = new HttpClient();
        public static double ElapsedMilliseconds { get; private set; } = 0;
        public bool Cancel { get; set; }
        private Database ReplayDatabase { get; set; }
        public AuthTokenInfo TokenInfo { get; }

        private int loadCount = 0;
        public Connection(AuthTokenInfo tokenInfo)
        {
            TokenInfo = tokenInfo;
            Client.DefaultRequestHeaders.Add("Authorization", tokenInfo.Token);
            ReplayDatabase = new Database();
        }
        public static AuthTokenInfo GetTokenInfo(string token)
        {
            var client = new HttpClient();
            var t = Task<string>.Run(async () =>
            {
                try
                {
                    client.DefaultRequestHeaders.Add("Authorization", token);
                    var response = await client.GetAsync("https://ballchasing.com/api/");
                    return await new StreamReader(await response.Content.ReadAsStreamAsync()).ReadToEndAsync();
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
        public async Task<ApiDataPack> CollectReplaysAsync(APIUrlBuilder builder)
        {
            OnProgressChange("Download started...");
            OnProgressUpdate(0);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            ApiDataPack dataPack = await GetDataPack(builder);
            sw.Stop();
            OnProgressChange("Downoald done...");
            OnProgressChange("Delete obsolete replays");
            dataPack.DeleteObsoleteReplays();
            OnProgressChange("Delete replays without specific names");
            ElapsedMilliseconds = sw.ElapsedMilliseconds;
            Cancel = false;
            GC.Collect();
            return dataPack;
        }

        private async Task<ApiDataPack> GetDataPack(APIUrlBuilder urlBuilder)
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
            while (!done)
            {
                HttpResponseMessage response = await Instance.Client.GetAsync(url);
                string dataString = await new StreamReader(await response.Content.ReadAsStreamAsync()).ReadToEndAsync();
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
            HttpResponseMessage response1 = await Instance.Client.GetAsync(url);
            string dataString1 = await new StreamReader(await response1.Content.ReadAsStreamAsync()).ReadToEndAsync();
            var currentPack1 = GetApiDataFromString(dataString1);
            return currentPack1.ReplayCount;
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
            for (int i = 0; i < replaysToDownload.Count; i++)
            {
                Replay r = replaysToDownload[i];
                try
                {
                    var replay = await GetAdvancedReplayInfosAsync(r);
                    //if (r.ID.Trim() != string.Empty)
                    //    ReplayDatabase.SaveReplayAsync(replay);
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

        private async Task<AdvancedReplay> GetAdvancedReplayInfosAsync(Replay replay)
        {
            var url = APIUrlBuilder.GetSpecificReplayUrl(replay.ID);
            HttpResponseMessage response = await Instance.Client.GetAsync(url);
            string dataString = await new StreamReader(await response.Content.ReadAsStreamAsync()).ReadToEndAsync();
            return GetAdvancedReplayFromString(dataString, replay);
        }

        private AdvancedReplay GetAdvancedReplayFromString(string dataString, Replay thisReplay)
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
