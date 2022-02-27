using System;
using RLStatsClasses.Models;
using System.Threading.Tasks;

namespace RLStatsClasses.Interfaces
{
    public interface IReplayProvider : IReplayProviderBase
    {
        Task<CollectReplaysResponse> CollectReplaysAsync(APIRequestFilter requestFilter, bool cached = false);
        void CancelDownload();
    }
}
