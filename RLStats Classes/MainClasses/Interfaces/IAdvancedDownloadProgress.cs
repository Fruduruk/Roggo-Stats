namespace RLStats_Classes.MainClasses.Interfaces
{
    public interface IAdvancedDownloadProgress
    {
        bool Initial { get; }
        int ChunksToDownload { get; }
        int DownloadedChunks { get; }
        int PacksToDownload { get; }
        int DownloadedPacks { get; }
        int ReplaysToDownload { get; }
        int DownloadedReplays { get; }
        string DownloadMessage { get; }
    }
}
