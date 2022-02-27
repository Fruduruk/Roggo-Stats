using RLStatsClasses.Models;

using System;

namespace RLStatsClasses.Interfaces
{
    public interface IReplayProviderBase
    {
        event EventHandler<ProgressState> DownloadProgressUpdated;
        string[] GetApiCalls();
        string[] GetAndDeleteApiCalls();
        void DeleteApiCalls();
    }
}
