using RLStatsClasses.Models.ReplayModels;
using RLStatsClasses.Models.ReplayModels.Advanced;

using System.Threading;
using System.Threading.Tasks;

namespace RLStatsClasses.Interfaces
{
    public interface IDatabase
    {
        int CacheSize { get; set; }
        int CacheHits { get; set; }
        int CacheMisses { get; set; }

        void ClearCache();
        bool IsReplayInDatabase(Replay replay);
        Task<AdvancedReplay> LoadReplayAsync(string id, CancellationToken cancellationToken);
        void SaveReplayAsync(AdvancedReplay replay);
    }
}
