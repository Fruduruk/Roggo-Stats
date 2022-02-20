using RLStats_Classes.Models;

using System;

namespace RLStats_Classes.MainClasses.Interfaces
{
    public interface IReplayProviderBase
    {
        event EventHandler<ProgressState> DownloadProgressUpdated;
        string[] GetApiCalls();
        string[] GetAndDeleteApiCalls();
        void DeleteApiCalls();
    }
}
