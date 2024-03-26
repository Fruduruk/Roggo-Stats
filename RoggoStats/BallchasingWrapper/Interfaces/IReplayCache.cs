using BallchasingWrapper.Models;
using BallchasingWrapper.Models.ReplayModels;

namespace BallchasingWrapper.Interfaces
{
    public interface IReplayCache
    {
        Task<HashSet<Replay>?> LoadCachedReplays(ApiUrlCreator filter);
        Task WriteReplayCache(ApiUrlCreator filter, IEnumerable<Replay> replays);
    }
}
