using BallchasingWrapper.BusinessLogic;
using BallchasingWrapper.Models;
using BallchasingWrapper.Models.ReplayModels;

namespace BallchasingWrapper.Interfaces
{
    public interface IReplayCache
    {
        void AddTheOtherReplaysToTheDataPack(HashSet<Replay> hashSet, APIRequestFilter filter);
        bool HasCacheFile(APIRequestFilter filter);
        bool HasOneReplayInFile(IEnumerable<Replay> replays, APIRequestFilter filter);
        void StoreReplaysInCache(IEnumerable<Replay> replays, APIRequestFilter filter);
    }
}
