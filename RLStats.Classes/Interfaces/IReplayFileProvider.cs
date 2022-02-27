
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RLStatsClasses.Interfaces
{
    public interface IReplayFileProvider
    {
        Task DownloadAndSaveReplayFileAsync(string fileName, string replayId);
        Task DownloadAndSaveReplayFilesAsync(string directoryName, IEnumerable<(string name, string id)> nameIdPairs, CancellationToken cancellationToken);
    }
}
