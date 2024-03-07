﻿using BallchasingWrapper.Interfaces;
using BallchasingWrapper.Models.ReplayModels;
using BallchasingWrapper.Models.ReplayModels.Advanced;

namespace BallchasingWrapper.BusinessLogic
{
    public class AdvancedReplayProvider : ReplayProviderBase, IAdvancedReplayProvider
    {
        private IDatabase ReplayDatabase { get; }

        public AdvancedReplayProvider(BallchasingApi api, IDatabase database, ILogger logger) : base(api, logger)
        {
            ReplayDatabase = database;
        }

        public async Task<IList<AdvancedReplay>> GetAdvancedReplayInfosAsync(IList<Replay> replays, bool singleThreaded = true)
        {
            var advancedReplays = new List<AdvancedReplay>();

            InitializeNewProgress();
            ProgressState.CurrentMessage = "Loading advanced replays from database.";
            var dbReplays = await ReplayDatabase.LoadReplaysAsync(replays.Select(r => r.Id), Api.StoppingToken, ProgressState);
            advancedReplays.AddRange(dbReplays);
            if (Api.StoppingToken.IsCancellationRequested)
                return new List<AdvancedReplay>();

            InitializeNewProgress();
            ProgressState.CurrentMessage = "Calculating replays to download.";
            var replaysToDownload = CalculateReplaysToDownload(replays, dbReplays);

            InitializeNewProgress();
            ProgressState.CurrentMessage = "Downloading advanced replays.";
            advancedReplays.AddRange(await DownloadReplays(replaysToDownload.ToList()));

            LastUpdateCall("Loading done.", advancedReplays.Count);
            if (Api.StoppingToken.IsCancellationRequested)
                return new List<AdvancedReplay>();
            return advancedReplays;
        }

        private static IEnumerable<Replay> CalculateReplaysToDownload(IEnumerable<Replay> replays, IEnumerable<AdvancedReplay> dbReplays)
        {
            var replaysToDownload = new List<Replay>();
            var idsInDatabase = dbReplays.Select(r => r.Id).ToList();
            foreach (var replay in replays)
            {
                if (!idsInDatabase.Contains(replay.Id))
                    replaysToDownload.Add(replay);
            }
            return replaysToDownload;
        }

        private async Task<IEnumerable<AdvancedReplay>> DownloadReplays(List<Replay> replaysToDownload)
        {
            var advancedReplays = new List<AdvancedReplay>();
            ProgressState.TotalCount = replaysToDownload.Count;
            for (int i = 0; i < replaysToDownload.Count; i++)
            {
                var r = replaysToDownload[i];
                var replay = await GetAdvancedReplayInfosAsync(r);
                advancedReplays.Add(replay);
                ReplayDatabase.SaveReplayAsync(replay);
                ProgressState.PartCount++;
                if (Api.StoppingToken.IsCancellationRequested)
                    break;
            }
            LastUpdateCall("Downlaoded advanced Replays", replaysToDownload.Count);
            return advancedReplays;
        }

        private async Task<AdvancedReplay> GetAdvancedReplayInfosAsync(Replay replay)
        {
            var url = ApiRequestBuilder.GetSpecificReplayUrl(replay.Id);
            var response = await Api.GetAsync(url);
            if (response.StatusCode == System.Net.HttpStatusCode.Locked)
                throw new OperationCanceledException();
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Couldn't load Advanced Replay: {response.ReasonPhrase}");
            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            var dataString = await reader.ReadToEndAsync();
            var advancedReplay = JsonConvert.DeserializeObject<AdvancedReplay>(dataString);
            return advancedReplay;
        }
    }
}
