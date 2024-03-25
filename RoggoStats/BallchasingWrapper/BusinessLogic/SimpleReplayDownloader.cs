using BallchasingWrapper.Interfaces;
using BallchasingWrapper.Models.ReplayModels;

namespace BallchasingWrapper.BusinessLogic;

public class SimpleReplayDownloader
{
    private readonly IBallchasingApi _api;
    private readonly string _startUrl;
    private string? _url;
    private readonly IReplayCache _replayCache;
    private readonly ILogger _logger;
    private int _index;
    private readonly List<Replay> _downloadedReplays = new();
    public bool EndReached { get; private set; }
    private int _count = -1;

    public SimpleReplayDownloader(IBallchasingApi api, string url, IReplayCache replayCache, ILogger logger)
    {
        _api = api;
        _startUrl = url;
        _url = url;
        _replayCache = replayCache;
        _logger = logger;
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken)
    {
        if (_count != -1)
            return _count;
        // just download 1 replay to save resources.
        var dataPack = await _api.GetApiDataPackAsync(_url + "&count=1", cancellationToken);
        _count = !dataPack.Success ? 0 : dataPack.ReplayCount;

        return _count;
    }

    public async Task<Replay?> GetNextReplayAsync(CancellationToken cancellationToken)
    {
        if (EndReached)
            return null;
        var replay = await DownloadReplayAtIndexAsync(_index, cancellationToken);
        if (cancellationToken.IsCancellationRequested)
        {
            return null;
        }

        if (replay is null)
            EndReached = true;
        _index++;
        return replay;
    }

    private async Task<Replay?> DownloadReplayAtIndexAsync(int index, CancellationToken cancellationToken)
    {
        while (!EndReached)
        {
            if (cancellationToken.IsCancellationRequested)
                return null;

            if (_downloadedReplays.Count - 1 >= index)
                return _downloadedReplays[index];

            await Task.Delay(10, cancellationToken);
        }

        return null;
    }

    public async Task StartBackgroundDownloadAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            if (_url is null || cancellationToken.IsCancellationRequested)
                break;
            _logger.LogInformation($"{((double)_downloadedReplays.Count / _count)*100}% Calling {_url}");

            var dataPack = await _api.GetApiDataPackAsync(_url, cancellationToken);
            _url = dataPack.Next;
            if (_count == -1)
                _count = dataPack.ReplayCount;

            if (!dataPack.Success || !dataPack.Replays.Any())
                break;
            _downloadedReplays.AddRange(dataPack.Replays);
        }
    }

    public void Reset()
    {
        _index = 0;
        EndReached = false;
    }
}