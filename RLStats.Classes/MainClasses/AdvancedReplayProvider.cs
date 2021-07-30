﻿using RLStats_Classes.AdvancedModels;
using RLStats_Classes.MainClasses.Interfaces;
using RLStats_Classes.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using static RLStats_Classes.MainClasses.TaskDisposer;

namespace RLStats_Classes.MainClasses
{
    public class AdvancedReplayProvider : ReplayProviderBase, IAdvancedReplayProvider
    {
        private Database ReplayDatabase { get; } = new Database();

        public AdvancedReplayProvider(IAuthTokenInfo tokenInfo) : base(tokenInfo) { }

        public async Task<IList<AdvancedReplay>> GetAdvancedReplayInfosAsync(IList<Replay> replays)
        {
            InitializeNewProgress();
            ReplayDatabase.CacheHits = 0;
            ReplayDatabase.CacheMisses = 0;
            var advancedReplays = new List<AdvancedReplay>();
            var replaysToDownload = new List<Replay>();
            var replaysToLoadFromDatabase = new List<Replay>();
            ProgressState.CurrentMessage = "Sorting advanced replays.";
            await SortReplays(new List<Replay>(replays), replaysToDownload, replaysToLoadFromDatabase);
            InitializeNewProgress();
            ProgressState.CurrentMessage = "Loading advanced replays from database.";
            var replaysToReDownload = await LoadReplays(advancedReplays, replaysToLoadFromDatabase);
            replaysToDownload.AddRange(replaysToReDownload);
            InitializeNewProgress();
            ProgressState.CurrentMessage = "Downloading advanced replays.";
            await DownloadReplays(advancedReplays, replaysToDownload);
            ProgressState.CurrentMessage = "Loading done.";
            ReplayDatabase.ReplayCache.Clear();
            ProgressState.CurrentMessage = $"Cache Hits: {ReplayDatabase.CacheHits} Cache Misses: {ReplayDatabase.CacheMisses}";
            return advancedReplays;
        }

        private async Task<List<Replay>> LoadReplays(List<AdvancedReplay> advancedReplays,
            List<Replay> replaysToLoadFromDatabase)
        {
            if (!replaysToLoadFromDatabase.Count.Equals(0))
            {
                ProgressState.TotalCount = replaysToLoadFromDatabase.Count;
                var loadedReplays = new List<AdvancedReplay>();
                var tasks = new List<Task<(bool, Replay)>>();
                (bool, Replay)[] results = Array.Empty<(bool, Replay)>();
                await Task.Run(async () =>
                {
                    foreach (var r in replaysToLoadFromDatabase)
                    {
                        var task = LoadAndAddToListAsync(loadedReplays, r);
                        tasks.Add(task);
                    }

                    results = await Task.WhenAll(tasks);
                });
                advancedReplays.AddRange(loadedReplays);
                var notLoadedReplays = new List<Replay>();
                foreach (var (success, replay) in results)
                {
                    if (success.Equals(false))
                    {
                        notLoadedReplays.Add(replay);
                    }
                }
                DisposeTasks(tasks);
                GC.Collect(3);
                return notLoadedReplays;
            }

            return new List<Replay>();
        }

        private async Task<(bool, Replay)> LoadAndAddToListAsync(List<AdvancedReplay> advancedReplays, Replay r)
        {
            var ar = await ReplayDatabase.LoadReplayAsync(r);
            
            if (ar is null)
            {
                ProgressState.FalsePartCount++;
                return (false, r);
            }
            lock (advancedReplays)
            {
                advancedReplays.Add(ar);
            }
            ProgressState.PartCount++;

            return (true, r);
        }

        private async Task DownloadReplays(List<AdvancedReplay> advancedReplays, List<Replay> replaysToDownload)
        {
            ProgressState.TotalCount = replaysToDownload.Count;
            for (int i = 0; i < replaysToDownload.Count; i++)
            {
                var r = replaysToDownload[i];
                var replay = await GetAdvancedReplayInfosAsync(r);
                advancedReplays.Add(replay);
                ReplayDatabase.SaveReplayAsync(replay);
                ProgressState.PartCount++;
            }
        }

        private async Task SortReplays(List<Replay> replays, List<Replay> replaysToDownload,
            List<Replay> replaysToLoadFromDatabase)
        {
            var count = replays.Count;
            ProgressState.TotalCount = count;
            ProgressState.FalsePartCount = 0;
            ProgressState.PartCount = 0;
            await Task.Run(() =>
            {
                foreach (var replay in replays)
                    SortAndAddToList(replaysToDownload, replaysToLoadFromDatabase, count, replay);
                while (replaysToDownload.Count + replaysToLoadFromDatabase.Count != replays.Count)
                {
                    Thread.Sleep(100);
                }
            });
        }

        private void SortAndAddToList(List<Replay> replaysToDownload, List<Replay> replaysToLoadFromDatabase,
            int count, Replay replay)
        {
            if (!ReplayDatabase.IsReplayInDatabase(replay))
            {
                lock (replaysToDownload)
                    replaysToDownload.Add(replay);
                ProgressState.FalsePartCount++;
            }
            else
            {
                lock (replaysToLoadFromDatabase)
                    replaysToLoadFromDatabase.Add(replay);
                ProgressState.PartCount++;
            }

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
