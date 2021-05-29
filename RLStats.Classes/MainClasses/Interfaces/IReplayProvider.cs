using System;
using RLStats_Classes.Models;
using System.Threading.Tasks;

namespace RLStats_Classes.MainClasses.Interfaces
{
    public interface IReplayProvider : IDownloadProgress
    {
        event EventHandler<IDownloadProgress> DownloadProgressUpdated;
        Task<CollectReplaysResponse> CollectReplaysAsync(APIRequestFilter requestFilter);
        void CancelDownload();
    }
}
