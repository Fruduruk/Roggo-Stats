using BallchasingWrapper.Interfaces;
using BallchasingWrapper.Models.ReplayModels;

namespace BallchasingWrapper.BusinessLogic;

public class SimpleReplayDownloader
{
    private readonly IBallchasingApi _api;
    private string? _url;
    private readonly IReplayCache _replayCache;
    private int _index;
    private readonly List<Replay> _downloadedReplays = new();
    public bool EndReached { get; private set; }
    private int _count = -1;

    public SimpleReplayDownloader(IBallchasingApi api, string url, IReplayCache replayCache)
    {
        _api = api;
        _url = url;
        _replayCache = replayCache;
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
        while (!EndReached)
        {
            if (_url is null || cancellationToken.IsCancellationRequested)
            {
                EndReached = true;
                break;
            }
            var dataPack = await _api.GetApiDataPackAsync(_url, cancellationToken);
            _url = dataPack.Next;
            if (_count == -1)
                _count = dataPack.ReplayCount;

            if (!dataPack.Success || !dataPack.Replays.Any())
                EndReached = true;
            else
                _downloadedReplays.AddRange(dataPack.Replays);
        }
    }
}