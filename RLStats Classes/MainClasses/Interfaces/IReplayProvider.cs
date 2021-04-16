using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RLStats_Classes.Models;

namespace RLStats_Classes.MainClasses.Interfaces
{
    public interface IReplayProvider
    {
        Task<ApiDataPack> CollectReplaysAsync(APIRequestFilter requestFilter);
        void CancelDownload();
    }
}
