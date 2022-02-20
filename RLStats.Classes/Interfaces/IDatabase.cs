using RLStats_Classes.Models.ReplayModels;
using RLStats_Classes.Models.ReplayModels.Advanced;

using System.Threading;
using System.Threading.Tasks;

namespace RLStats_Classes.Interfaces
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
