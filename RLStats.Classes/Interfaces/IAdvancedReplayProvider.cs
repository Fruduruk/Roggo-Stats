using RLStatsClasses.Models.ReplayModels;
using RLStatsClasses.Models.ReplayModels.Advanced;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RLStatsClasses.Interfaces
{
    public interface IAdvancedReplayProvider : IReplayProviderBase
    {
        Task<IList<AdvancedReplay>> GetAdvancedReplayInfosAsync(IList<Replay> replays, bool singleThreaded = false);
    }
}
