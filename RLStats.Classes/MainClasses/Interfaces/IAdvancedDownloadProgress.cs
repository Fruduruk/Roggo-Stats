namespace RLStats_Classes.MainClasses.Interfaces
{
    public interface IAdvancedDownloadProgress
    {
        bool Initial { get; }
        int ReplaysToDownload { get; }
        int DownloadedReplays { get; }
        string DownloadMessage { get; }
    }
}
