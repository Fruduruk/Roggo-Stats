using BallchasingWrapper.Interfaces;

namespace BallchasingWrapper.BusinessLogic;

public class BackgroundDownloadingService
{
    private readonly IBallchasingApi _api;
    private readonly IReplayCache _replayCache;
    private readonly IDatabase _database;
    private readonly IBackgroundDownloaderConfig _backgroundDownloaderConfig;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<BackgroundDownloadingService> _logger;
    private readonly List<BackgroundReplayDownloader> _downloaders = new();

    public BackgroundDownloadingService(IBallchasingApi api, IReplayCache replayCache, IDatabase database,
        IBackgroundDownloaderConfig backgroundDownloaderConfig, IHostApplicationLifetime lifetime,
        ILogger<BackgroundDownloadingService> logger)
    {
        _api = api;
        _replayCache = replayCache;
        _database = database;
        _backgroundDownloaderConfig = backgroundDownloaderConfig;
        _lifetime = lifetime;
        _logger = logger;
    }

    public void StartBackgroundDownload()
    {
        StartBackgroundDownloadAsync();
        _logger.LogInformation("Started Background download task...");
    }

    private async void StartBackgroundDownloadAsync()
    {
        var operations = (await _backgroundDownloaderConfig.LoadOperationsAsync()).ToList();
        await Task.WhenAll(operations.Select(operation =>
        {
            var downloader = new BackgroundReplayDownloader(_api, _replayCache, _database, operation.Identity,
                operation.CycleIntervalInHours, _logger);
            _downloaders.Add(downloader);
            return downloader.RepeatedlyDownloadAdvancedReplaysAsync(_lifetime.ApplicationStopping);
        }).ToList());
        _logger.LogInformation("All background tasks done...");
    }

    private void CancelAllBackgroundDownloaders()
    {
        foreach (var downloader in _downloaders)
        {
            downloader.Cancel();
        }
        _downloaders.Clear();
    }

    public async Task<bool> InsertNewBackgroundDownloadAsync(Grpc.BackgroundDownloadOperation operation)
    {
        var success = await _backgroundDownloaderConfig.SaveOperationAsync(operation);
        if (!success)
            return false;

        CancelAllBackgroundDownloaders();
        StartBackgroundDownloadAsync();
        return true;
    }
}