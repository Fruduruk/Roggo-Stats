using RLStats_Classes.AdvancedModels;
using RLStats_Classes.MainClasses.Interfaces;
using RLStats_Classes.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RLStats_Classes.MainClasses
{
    public class AdvancedReplayProvider : ReplayProviderBase, IAdvancedReplayProvider
    {
        private Database ReplayDatabase { get; } = new Database();

        public AdvancedReplayProvider(IAuthTokenInfo tokenInfo) : base(tokenInfo) { }

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
            return advancedReplays;
        }

        private async Task<List<Replay>> LoadReplays(List<AdvancedReplay> advancedReplays,
            List<Replay> replaysToLoadFromDatabase)
        {
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
            for (int i = 0; i < replaysToDownload.Count; i++)
            {
                var r = replaysToDownload[i];
                var replay = await GetAdvancedReplayInfosAsync(r);
                advancedReplays.Add(replay);
                ReplayDatabase.SaveReplayAsync(replay);
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

            if (currentCount != count)
                return;
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
