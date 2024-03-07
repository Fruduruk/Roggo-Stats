using BallchasingWrapper.Interfaces;

namespace BallchasingWrapper.BusinessLogic
{
    public class ReplayFileProvider : ReplayProviderBase, IReplayFileProvider
    {
        public ReplayFileProvider(BallchasingApi api, ILogger logger) : base(api, logger)
        {
        }

        public async Task DownloadAndSaveReplayFileAsync(string fileName, string replayId,
            CancellationToken cancellationToken)
        {
            Api.MaximumCallsPerSecond = 2;
            var fileUrl = ApiRequestBuilder.GetReplayFileUrl(replayId);
            using var response = await Api.GetAsync(fileUrl,cancellationToken);
            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            await File.WriteAllBytesAsync(fileName, bytes, cancellationToken);
            Api.MaximumCallsPerSecond = 1000;
        }

        public async Task DownloadAndSaveReplayFilesAsync(string directoryName, IEnumerable<(string name, string id)> nameIdPairs, CancellationToken cancellationToken)
        {
            InitializeNewProgress();
            var pairList = new List<(string name, string id)>(nameIdPairs);
            ProgressState.CurrentMessage = $"Downloading replay files";
            ProgressState.TotalCount = pairList.Count;
            foreach (var (name, id) in pairList)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                var fileName = $"{name}.replay";
                var filePath = Path.Combine(directoryName, fileName);
                if (!File.Exists(filePath))
                    await DownloadAndSaveReplayFileAsync(filePath, id,cancellationToken);
                ProgressState.PartCount++;
            }
            ProgressState.CurrentMessage = $"Successfully downloaded {ProgressState.PartCount} replay files";
            ProgressState.LastCall = true;
        }
    }
}
