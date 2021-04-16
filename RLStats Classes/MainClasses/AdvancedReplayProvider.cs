using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RLStats_Classes.AdvancedModels;
using RLStats_Classes.MainClasses.Interfaces;
using RLStats_Classes.Models;

namespace RLStats_Classes.MainClasses
{
    public class AdvancedReplayProvider : ReplayProviderBase, IAdvancedReplayProvider, IAdvancedDownloadProgress
    {
        public static event EventHandler<IAdvancedDownloadProgress> AdvancedDownloadProgressUpdated;
        public bool Initial { get; private set; }
        public int ReplaysToDownload { get; private set; }
        public int DownloadedReplays { get; private set; }
        public string DownloadMessage { get; private set; }
        private Database ReplayDatabase { get; } = new Database();

        public AdvancedReplayProvider(IAuthTokenInfo tokenInfo) : base(tokenInfo)
        {
        }

        private void OnAdvancedDownloadProgressUpdate(IAdvancedDownloadProgress advancedDownloadProgress)
        {
            AdvancedDownloadProgressUpdated?.Invoke(this, advancedDownloadProgress);
        }

        private void ClearProgressUpdateVariables()
        {
            ReplaysToDownload = 0;
            DownloadedReplays = 0;
            DownloadMessage = string.Empty;
        }

        public async Task<IList<AdvancedReplay>> GetAdvancedReplayInfosAsync(IList<Replay> replays)
        {
            ReplayDatabase.CacheHits = 0;
            ReplayDatabase.CacheMisses = 0;
            var advancedReplays = new List<AdvancedReplay>();
            var replaysToDownload = new List<Replay>();
            var replaysToLoadFromDatabase = new List<Replay>();
            await SortReplays(new List<Replay>(replays), replaysToDownload, replaysToLoadFromDatabase);
            var replaysToReDownload = await LoadReplays(advancedReplays, replaysToLoadFromDatabase);
            replaysToDownload.AddRange(replaysToReDownload);
            await DownloadReplays(advancedReplays, replaysToDownload);
            ReplayDatabase.ReplayCache.Clear();
            DownloadMessage =
                $"Replays loaded: {advancedReplays.Count}\tcache hits: {ReplayDatabase.CacheHits}\tcache misses: {ReplayDatabase.CacheMisses}";
            OnAdvancedDownloadProgressUpdate(this);
            return advancedReplays;
        }

        private async Task<List<Replay>> LoadReplays(List<AdvancedReplay> advancedReplays,
            List<Replay> replaysToLoadFromDatabase)
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

        private async Task<(bool, Replay)> LoadAndAddToListAsync(List<AdvancedReplay> advancedReplays, Replay r,
            int totalCount)
        {
            if ((advancedReplays.Count % 10 == 0) || advancedReplays.Count.Equals(totalCount))
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
            for (int i = 0; i < replaysToDownload.Count; i++)
            {
                var r = replaysToDownload[i];
                try
                {
                    var replay = await GetAdvancedReplayInfosAsync(r);
                    advancedReplays.Add(replay);
                    ReplayDatabase.SaveReplayAsync(replay);

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

        private async Task SortReplays(List<Replay> replays, List<Replay> replaysToDownload,
            List<Replay> replaysToLoadFromDatabase)
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

        private void SortAndAddToListAsync(List<Replay> replaysToDownload, List<Replay> replaysToLoadFromDatabase,
            int count, Replay replay)
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
                OnAdvancedDownloadProgressUpdate(this);
            }

            if (currentCount != count)
                return;
            //ProgressInPercent = (Convert.ToDouble(currentCount) / Convert.ToDouble(count)) * 100;
            DownloadMessage = $"Sort Replays: {currentCount}/{count}";
            OnAdvancedDownloadProgressUpdate(this);
        }

        private async Task<AdvancedReplay> GetAdvancedReplayInfosAsync(Replay replay)
        {
            var url = APIRequestBuilder.GetSpecificReplayUrl(replay.Id);
            var response = await Api.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Couldn't load Advanced Replay: {response.ReasonPhrase}");
            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            var dataString = await reader.ReadToEndAsync();
            return AdvancedReplayAssembler.GetAdvancedReplayFromString(dataString);
        }
    }
}
