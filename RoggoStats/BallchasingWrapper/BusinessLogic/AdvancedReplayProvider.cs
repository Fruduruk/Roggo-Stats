using BallchasingWrapper.Interfaces;
using BallchasingWrapper.Models.ReplayModels;
using BallchasingWrapper.Models.ReplayModels.Advanced;

namespace BallchasingWrapper.BusinessLogic
{
    public class AdvancedReplayProvider
    {
        private readonly BallchasingApi _api;
        private readonly ILogger _logger;
        private IDatabase ReplayDatabase { get; }

        public AdvancedReplayProvider(BallchasingApi api, IDatabase database, ILogger logger)
        {
            _api = api;
            _logger = logger;
            ReplayDatabase = database;
        }

        public async Task<IList<AdvancedReplay>> GetAdvancedReplayInfosAsync(IList<Replay> replays,
            CancellationToken cancellationToken)
        {
            var advancedReplays = new List<AdvancedReplay>();

            _logger.LogInformation("Loading advanced replays from database.");
            var dbReplays =
                await ReplayDatabase.LoadReplaysAsync(replays.Select(r => r.Id), cancellationToken);
            advancedReplays.AddRange(dbReplays);
            if (cancellationToken.IsCancellationRequested)
                return new List<AdvancedReplay>();

            _logger.LogInformation("Calculating replays to download.");
            var replaysToDownload = CalculateReplaysToDownload(replays, dbReplays);

            _logger.LogInformation("Downloading advanced replays.");
            advancedReplays.AddRange(await DownloadReplays(replaysToDownload.ToList(), cancellationToken));

            _logger.LogInformation("Loading advanced replays done.");
            if (cancellationToken.IsCancellationRequested)
                return new List<AdvancedReplay>();
            return advancedReplays;
        }

        private static IEnumerable<Replay> CalculateReplaysToDownload(IEnumerable<Replay> replays,
            IEnumerable<AdvancedReplay> dbReplays)
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

        private async Task<IEnumerable<AdvancedReplay>> DownloadReplays(List<Replay> replaysToDownload,
            CancellationToken cancellationToken)
        {
            var advancedReplays = new List<AdvancedReplay>();
            for (int i = 0; i < replaysToDownload.Count; i++)
            {
                var r = replaysToDownload[i];
                var replay = await GetAdvancedReplayInfosAsync(r, cancellationToken);
                advancedReplays.Add(replay);
                ReplayDatabase.SaveReplayAsync(replay);
                if (cancellationToken.IsCancellationRequested)
                    break;
            }

            _logger.LogInformation("Downlaoded advanced Replays");
            return advancedReplays;
        }

        private async Task<AdvancedReplay> GetAdvancedReplayInfosAsync(Replay replay,
            CancellationToken cancellationToken)
        {
            var url = ApiRequestBuilder.GetSpecificReplayUrl(replay.Id);
            var response = await _api.GetAsync(url, cancellationToken);
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