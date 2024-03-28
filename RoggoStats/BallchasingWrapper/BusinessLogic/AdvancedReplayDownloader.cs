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
    
    public async Task<AdvancedReplay?> DownloadAdvancedReplayById(string id, CancellationToken cancellationToken)
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
        File.WriteAllText("new advanced replay.json",dataString);
        var advancedReplay =  cancellationToken.IsCancellationRequested ? 
            null : JsonConvert.DeserializeObject<AdvancedReplay>(dataString);
        return advancedReplay;
    }
}