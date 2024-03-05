using BallchasingWrapper.Models;

namespace BallchasingWrapper.Interfaces
{
    public interface IReplayProviderBase
    {
        event EventHandler<ProgressState> DownloadProgressUpdated;
        string[] GetApiCalls();
        string[] GetAndDeleteApiCalls();
        void DeleteApiCalls();
    }
}
