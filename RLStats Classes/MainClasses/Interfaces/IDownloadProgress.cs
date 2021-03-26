namespace RLStats_Classes.MainClasses.Interfaces
{
    public interface IDownloadProgress
    {
        bool Initial { get; }
        int ChunksToDownload { get; }
        int DownloadedChunks { get; }
        int PacksToDownload { get; }
        int DownloadedPacks { get; }
        string DownloadMessage { get; }
    }
}
