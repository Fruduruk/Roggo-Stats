using RLStats_Classes.Models.ReplayModels;

using System.Collections.Generic;

namespace RLStats_Classes.Interfaces
{
    public interface IReplayCache
    {
        void AddTheOtherReplaysToTheDataPack(HashSet<Replay> hashSet, APIRequestFilter filter);
        bool HasCacheFile(APIRequestFilter filter);
        bool HasOneReplayInFile(IEnumerable<Replay> replays, APIRequestFilter filter);
        void StoreReplaysInCache(IEnumerable<Replay> replays, APIRequestFilter filter);
    }
}
