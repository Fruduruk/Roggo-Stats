using RLStatsClasses.Models.ReplayModels;

using System.Collections.Generic;

namespace RLStatsClasses.Interfaces
{
    public interface IReplayCache
    {
        void AddTheOtherReplaysToTheDataPack(HashSet<Replay> hashSet, APIRequestFilter filter);
        bool HasCacheFile(APIRequestFilter filter);
        bool HasOneReplayInFile(IEnumerable<Replay> replays, APIRequestFilter filter);
        void StoreReplaysInCache(IEnumerable<Replay> replays, APIRequestFilter filter);
    }
}
