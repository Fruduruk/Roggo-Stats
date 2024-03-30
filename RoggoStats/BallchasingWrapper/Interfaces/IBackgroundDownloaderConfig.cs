namespace BallchasingWrapper.Interfaces;

public interface IBackgroundDownloaderConfig
{
    Task<IEnumerable<Grpc.BackgroundDownloadOperation>> LoadOperationsAsync();
    Task<bool> SaveOperationAsync(Grpc.BackgroundDownloadOperation operation);
    Task<bool> DeleteOperationAsync(Grpc.Identity identity);
}