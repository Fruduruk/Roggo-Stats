using System;
using RLStats_Classes.Models;
using System.Threading.Tasks;

namespace RLStats_Classes.Interfaces
{
    public interface IReplayProvider : IReplayProviderBase
    {
        Task<CollectReplaysResponse> CollectReplaysAsync(APIRequestFilter requestFilter, bool cached = false);
        void CancelDownload();
    }
}
