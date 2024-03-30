﻿using BallchasingWrapper.Interfaces;
using BallchasingWrapper.Models;

namespace BallchasingWrapper.BusinessLogic;

public class BackgroundReplayDownloader
{
    private readonly IBallchasingApi _api;
    private readonly IReplayCache _replayCache;
    private readonly IDatabase _database;
    public Grpc.Identity Identity { get; }
    private readonly double _cycleIntervalInHours;
    private readonly ILogger _logger;
    private readonly CancellationTokenSource _manualCancellationTokenSource;

    public BackgroundReplayDownloader(IBallchasingApi api, IReplayCache replayCache, IDatabase database,
        Grpc.Identity identity, double cycleIntervalInHours, ILogger logger)
    {
        _api = api;
        _replayCache = replayCache;
        _database = database;
        Identity = identity;
        _cycleIntervalInHours = cycleIntervalInHours;
        _logger = logger;
        _manualCancellationTokenSource = new CancellationTokenSource();
    }

    public async Task RepeatedlyDownloadAdvancedReplaysAsync(CancellationToken cancellationToken)
    {
        var combinedToken =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _manualCancellationTokenSource.Token)
                .Token;
        while (true)
        {
            await Task.Delay(TimeSpan.FromSeconds(_cycleIntervalInHours), combinedToken);
            if (combinedToken.IsCancellationRequested)
                break;
            _logger.LogInformation("Downloaded for " + Identity.NameOrId);
            await StartDownloadCycle(combinedToken);
            if (combinedToken.IsCancellationRequested)
                break;
        }
    }

    private async Task StartDownloadCycle(CancellationToken cancellationToken)
    {
        var urlCreator = ApiUrlCreator.CreateSimpleIdentityFilter(Identity);
        var collector = new ReplayCollector(urlCreator, _api, _replayCache, _logger);
        var ids = (await collector.CollectReplaysAsync(_logger, cancellationToken))
            .Replays.Select(replay => replay.Id ?? string.Empty).Where(id => !string.IsNullOrWhiteSpace(id));

        if (cancellationToken.IsCancellationRequested)
            return;

        // Shorten for debugging
        ids = new[] { ids.First() };
        // remove after debugging
        
        var downloader = new AdvancedReplayDownloader(_api, _database, _logger);
        var advancedReplays =
            await downloader.LoadAdvancedReplaysByIdsAsync(ids, cancellationToken);
    }

    public void Cancel()
    {
        if (!_manualCancellationTokenSource.Token.IsCancellationRequested)
            _manualCancellationTokenSource.Cancel();
    }
}