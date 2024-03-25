﻿using BallchasingWrapper.Interfaces;
using BallchasingWrapper.Models;

namespace BallchasingWrapper.BusinessLogic;

public class ReplayCollectorFactory
{
    private readonly IBallchasingApi _api;
    private readonly IReplayCache _replayCache;
    private ILogger _logger;
    private readonly Dictionary<int, ReplayCollector> _collectors = new();

    public ReplayCollectorFactory(IBallchasingApi api, IReplayCache replayCache)
    {
        _api = api;
        _replayCache = replayCache;
    }

    public void AddCurrentLogger(ILogger logger)
    {
        _logger = logger;
    }

    public ReplayCollector GetReplayCollector(ApiUrlCreator urlCreator)
    {
        if (_collectors.ContainsKey(urlCreator.GetHashCode()))
            return _collectors[urlCreator.GetHashCode()];
        var collector = new ReplayCollector(urlCreator,_api, _replayCache,_logger);
        _collectors.Add(urlCreator.GetHashCode(),collector);
        return collector;
    }

    public void Shutdown()
    {
        foreach (var (_,collector) in _collectors)
        {
            collector.Dispose();
        }
    }
}