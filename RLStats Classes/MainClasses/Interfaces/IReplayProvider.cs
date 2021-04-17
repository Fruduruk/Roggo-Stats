using RLStats_Classes.Models;
using System.Threading.Tasks;

namespace RLStats_Classes.MainClasses.Interfaces
{
    public interface IReplayProvider
    {
        Task<ApiDataPack> CollectReplaysAsync(APIRequestFilter requestFilter);
        void CancelDownload();
    }
}
