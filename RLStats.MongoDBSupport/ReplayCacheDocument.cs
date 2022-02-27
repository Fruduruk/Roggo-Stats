using RLStatsClasses.Models.ReplayModels;

namespace RLStats.MongoDBSupport
{
    public class ReplayCacheDocument
    {
        public string Url { get; set; } = null!;
        public IEnumerable<Replay> Replays { get; set; } = null!;
    }
}
