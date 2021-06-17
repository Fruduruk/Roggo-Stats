using System;
using RLStats_Classes.Models;
using System.Threading.Tasks;

namespace RLStats_Classes.MainClasses.Interfaces
{
    public interface IReplayProvider
    {
        event EventHandler<ProgressState> DownloadProgressUpdated;
        Task<CollectReplaysResponse> CollectReplaysAsync(APIRequestFilter requestFilter);
        void CancelDownload();
    }
}
