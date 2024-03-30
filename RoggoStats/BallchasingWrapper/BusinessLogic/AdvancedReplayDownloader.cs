using BallchasingWrapper.Interfaces;
using BallchasingWrapper.Models.ReplayModels.Advanced;

namespace BallchasingWrapper.BusinessLogic;

public class AdvancedReplayDownloader
{
    private readonly IBallchasingApi _api;
    private readonly ILogger _logger;
    private readonly IDatabase _replayDatabase;

    public AdvancedReplayDownloader(IBallchasingApi api, IDatabase database, ILogger logger)
    {
        _api = api;
        _logger = logger;
        _replayDatabase = database;
    }

    public async Task<IEnumerable<AdvancedReplay>> LoadAdvancedReplaysByIdsAsync(IEnumerable<string> ids,
        CancellationToken cancellationToken)
    {
        try
        {
            var idList = ids.ToList();
            var cachedAdvancedReplays =
                (await _replayDatabase.LoadReplaysByIdsAsync(idList, cancellationToken)).ToList();
            if (cancellationToken.IsCancellationRequested)
                return Array.Empty<AdvancedReplay>();

            var downloadedReplays = new List<AdvancedReplay>();

            _logger.LogInformation($"Found {cachedAdvancedReplays.Count}/{idList.Count} advanced replays in cache.");
            var idsToDownload = idList
                .Where(id => !cachedAdvancedReplays
                    .Select(replay => replay.Id)
                    .Contains(id))
                .ToList();
            _logger.LogInformation($"Downloading {idsToDownload.Count} advanced replays...");
            int count = 0;
            foreach (var id in idsToDownload)
            {
                _logger.LogInformation($"Downloading replay {++count}/{idsToDownload.Count} {id}...");
                var downloadedReplay = await LoadAdvancedReplayByIdAsync(id, cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                    return Array.Empty<AdvancedReplay>();
                if (downloadedReplay is null)
                    throw new Exception("Replay was null but cancellation is not requested.");
                downloadedReplays.Add(downloadedReplay);
                await _replayDatabase.SaveReplayAsync(downloadedReplay);
            }

            return cachedAdvancedReplays.Concat(downloadedReplays)
                .OrderByDescending(advancedReplay => advancedReplay.Created);
        }
        catch (OperationCanceledException)
        {
            return new List<AdvancedReplay>();
        }
    }

    public async Task<AdvancedReplay?> LoadAdvancedReplayByIdAsync(string id, CancellationToken cancellationToken)
    {
        try
        {
            var cachedAdvancedReplay =
                (await _replayDatabase.LoadReplaysByIdsAsync(new[] { id }, cancellationToken)).FirstOrDefault();
            if (cancellationToken.IsCancellationRequested)
                return null;
            if (cachedAdvancedReplay is not null)
                return cachedAdvancedReplay;

            var downloadedReplay = await DownloadAdvancedReplayByIdAsync(id, cancellationToken);
            return downloadedReplay;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }

    private async Task<AdvancedReplay?> DownloadAdvancedReplayByIdAsync(string id,
        CancellationToken cancellationToken = default)
    {
        var url = ApiRequestBuilder.GetSpecificReplayUrl(id);
        var response = await _api.GetAsync(url, cancellationToken);
        if (cancellationToken.IsCancellationRequested)
            return null;
        if (response.StatusCode == System.Net.HttpStatusCode.Locked)
            throw new OperationCanceledException();
        if (!response.IsSuccessStatusCode)
            throw new Exception($"Couldn't load Advanced Replay: {response.ReasonPhrase}");
        using var reader = new StreamReader(await response.Content.ReadAsStreamAsync(cancellationToken));
        if (cancellationToken.IsCancellationRequested)
            return null;
        var dataString = await reader.ReadToEndAsync(cancellationToken);
        return cancellationToken.IsCancellationRequested
            ? null
            : JsonConvert.DeserializeObject<AdvancedReplay>(dataString);
    }
}