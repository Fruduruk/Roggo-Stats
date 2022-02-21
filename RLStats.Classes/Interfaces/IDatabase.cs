using RLStatsClasses.Models.ReplayModels.Advanced;

using System.Collections.Generic;
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

        /// <summary>
        /// Loads all replays that are saved in the database
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<AdvancedReplay>> LoadReplaysAsync(IEnumerable<string> ids, CancellationToken cancellationToken);

        /// <summary>
        /// Inserts one replay into the database
        /// </summary>
        /// <param name="replay"></param>
        void SaveReplayAsync(AdvancedReplay replay);
    }
}
