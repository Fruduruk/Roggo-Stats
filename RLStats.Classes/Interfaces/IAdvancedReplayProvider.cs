using RLStats_Classes.Models.ReplayModels;
using RLStats_Classes.Models.ReplayModels.Advanced;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RLStats_Classes.Interfaces
{
    public interface IAdvancedReplayProvider : IReplayProviderBase
    {
        Task<IList<AdvancedReplay>> GetAdvancedReplayInfosAsync(IList<Replay> replays, bool singleThreaded = false);
    }
}
