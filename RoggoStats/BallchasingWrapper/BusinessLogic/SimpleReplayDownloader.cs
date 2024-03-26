using BallchasingWrapper.Interfaces;
using BallchasingWrapper.Models.ReplayModels;

namespace BallchasingWrapper.BusinessLogic;

public class SimpleReplayDownloader
{
    private readonly IBallchasingApi _api;
    public string StartUrl { get; }
    private string? _url;
    private readonly ILogger _logger;
    private int _index;
    private readonly List<Replay> _downloadedReplays = new();
    private readonly Stack<Replay> _triedReplays = new();
    public List<Replay> ReplaysProvided { get; } = new();
    public bool EndReached { get; private set; }
    private int _count = -1;

    public SimpleReplayDownloader(IBallchasingApi api, string url, ILogger logger)
    {
        _api = api;
        StartUrl = url;
        _url = url;
        _logger = logger;
    }

    public async Task<Replay?> GetNextReplayAsync(CancellationToken cancellationToken)
    {
        if (EndReached)
            return null;

        var replay = await DownloadReplayAtIndexAsync(_index, cancellationToken);
        if (cancellationToken.IsCancellationRequested)
            return null;

        if (replay is null)
            EndReached = true;
        else
            _triedReplays.Push(replay);
        _index++;
        
        return replay;
    }

    private async Task<Replay?> DownloadReplayAtIndexAsync(int index, CancellationToken cancellationToken)
    {
        if (_downloadedReplays.Count > index)
            return _downloadedReplays[index];

        if (_url is null || cancellationToken.IsCancellationRequested)
            return null;
        if (_count != 0)
            _logger.LogInformation($"{((double)_downloadedReplays.Count / _count) * 100}% Calling {_url}");

        var dataPack = await _api.GetApiDataPackAsync(_url, cancellationToken);
        _url = dataPack.Next;
        if (_count == -1)
            _count = dataPack.ReplayCount;

        if (!dataPack.Success || !dataPack.Replays.Any())
            return null;
        _downloadedReplays.AddRange(dataPack.Replays);
        return _downloadedReplays[index];
    }

    public void IncrementReplaysProvided()
    {
        if (!_triedReplays.Any())
            throw new Exception($"You cannot use {nameof(IncrementReplaysProvided)} if there were no new replays provided.");
        
        ReplaysProvided.Add(_triedReplays.Pop());
    }
}