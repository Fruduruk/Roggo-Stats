using RLStats_Classes.AdvancedModels;
using RLStats_Classes.Models;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RLStats_Classes.MainClasses.Interfaces
{
    public interface IAdvancedReplayProvider : IReplayProviderBase
    {
        Task<IList<AdvancedReplay>> GetAdvancedReplayInfosAsync(IList<Replay> replays, bool singleThreaded = false);
    }
}
