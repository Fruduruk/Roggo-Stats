using BallchasingWrapper.Interfaces;
using BallchasingWrapper.Models.ReplayModels;

namespace BallchasingWrapper.BusinessLogic;

public class SimpleReplayDownloader
{
    private readonly IBallchasingApi _api;
    private string? _url;
    private readonly CancellationToken _cancellationToken;
    private int _index;
    private readonly List<Replay> _downloadedReplays = new();
    public bool EndReached { get; private set; }
    private int _count = -1;

    public SimpleReplayDownloader(IBallchasingApi api, string url, CancellationToken cancellationToken)
    {
        _api = api;
        _url = url;
        _cancellationToken = cancellationToken;
    }

    public async Task<int> GetCountAsync()
    {
        if (_count != -1)
            return _count;
        // just download 1 replay to save resources.
        var dataPack = await _api.GetApiDataPackAsync(_url + "&count=1", _cancellationToken);
        _count = !dataPack.Success ? 0 : dataPack.ReplayCount;

        return _count;
    }
    
    public async Task<Replay?> DownloadNextReplayAsync()
    {
        if (EndReached || _cancellationToken.IsCancellationRequested)
            return null;
        var replay = await DownloadReplayAtIndexAsync(_index);
        if (replay is null)
            EndReached = true;
        _index++;
        return replay;
    }

    private async Task<Replay?> DownloadReplayAtIndexAsync(int index)
    {
        if (_downloadedReplays.Count - 1 >= index)
            return _downloadedReplays[index];
        
        if (_url is null)
            return null;
        var dataPack = await _api.GetApiDataPackAsync(_url, _cancellationToken);
        if (!dataPack.Success)
            return null;
        _url = dataPack.Next;
        if (!dataPack.Replays.Any())
            return null;
        _count = dataPack.ReplayCount;
        _downloadedReplays.AddRange(dataPack.Replays);
        return _downloadedReplays[index];
    }
}