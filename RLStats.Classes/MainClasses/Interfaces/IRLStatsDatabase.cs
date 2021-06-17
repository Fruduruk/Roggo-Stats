using RLStats_Classes.AdvancedModels;
using RLStats_Classes.Models;

using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace RLStats_Classes.MainClasses.Interfaces
{
    public interface IRLStatsDatabase
    {
        int CacheHits { get; set; }
        int CacheMisses { get; set; }
        public ObservableCollection<AdvancedReplay> ReplayCache { get; set; }

        Task<AdvancedReplay> LoadReplayAsync(Replay r);
        void SaveReplayAsync(AdvancedReplay replay);
        bool IsReplayInDatabase(Replay replay);
    }
}
