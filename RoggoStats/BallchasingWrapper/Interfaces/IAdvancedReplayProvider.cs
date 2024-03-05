using BallchasingWrapper.Models.ReplayModels;
using BallchasingWrapper.Models.ReplayModels.Advanced;

namespace BallchasingWrapper.Interfaces
{
    public interface IAdvancedReplayProvider : IReplayProviderBase
    {
        Task<IList<AdvancedReplay>> GetAdvancedReplayInfosAsync(IList<Replay> replays, bool singleThreaded = false);
    }
}
