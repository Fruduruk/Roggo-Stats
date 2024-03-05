using BallchasingWrapper.BusinessLogic;
using BallchasingWrapper.Models;

namespace BallchasingWrapper.Interfaces
{
    public interface IReplayProvider : IReplayProviderBase
    {
        Task<CollectReplaysResponse> CollectReplaysAsync(APIRequestFilter requestFilter, bool cached = false);
        void CancelDownload();
    }
}
