using Microsoft.Extensions.Logging;

using RLStats_Classes.MainClasses.Interfaces;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RLStats_Classes.MainClasses
{
    public class ReplayFileProvider : ReplayProviderBase, IReplayFileProvider
    {
        public ReplayFileProvider(IAuthTokenInfo tokenInfo, ILogger logger) : base(tokenInfo, logger)
        {
        }

        public async Task DownloadAndSaveReplayFileAsync(string fileName, string replayId)
        {
            Api.MaximumCallsPerSecond = 2;
            var fileUrl = APIRequestBuilder.GetReplayFileUrl(replayId);
            using var response = await Api.GetAsync(fileUrl);
            var bytes = await response.Content.ReadAsByteArrayAsync();
            await File.WriteAllBytesAsync(fileName, bytes);
            Api.MaximumCallsPerSecond = 1000;
        }

        public async Task DownloadAndSaveReplayFilesAsync(string directoryName, IEnumerable<(string name, string id)> nameIdPairs, CancellationToken cancellationToken)
        {
            InitializeNewProgress();
            var pairList = new List<(string name, string id)>(nameIdPairs);
            ProgressState.CurrentMessage = $"Downloading replay files";
            ProgressState.TotalCount = pairList.Count;
            Api.StoppingToken = cancellationToken;
            foreach (var (name, id) in pairList)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                var fileName = $"{name}.replay";
                var filePath = Path.Combine(directoryName, fileName);
                if (!File.Exists(filePath))
                    await DownloadAndSaveReplayFileAsync(filePath, id);
                ProgressState.PartCount++;
            }
            ProgressState.CurrentMessage = $"Successfully downloaded {ProgressState.PartCount} replay files";
            ProgressState.LastCall = true;
        }
    }
}
