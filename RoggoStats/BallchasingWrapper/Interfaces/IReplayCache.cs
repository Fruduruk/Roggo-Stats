using BallchasingWrapper.BusinessLogic;
using BallchasingWrapper.Models;
using BallchasingWrapper.Models.ReplayModels;

namespace BallchasingWrapper.Interfaces
{
    public interface IReplayCache
    {
        void AddTheOtherReplaysToTheDataPack(HashSet<Replay> hashSet, ApiUrlCreator filter);
        bool HasCacheFile(ApiUrlCreator filter);
        bool HasOneReplayInFile(IEnumerable<Replay> replays, ApiUrlCreator filter);
        void StoreReplaysInCache(IEnumerable<Replay> replays, ApiUrlCreator filter);
    }
}
