using BallchasingWrapper.Models;
using BallchasingWrapper.Models.ReplayModels.Advanced;

namespace BallchasingWrapper.Interfaces
{
    public interface IDatabase
    {
        /// <summary>
        /// Loads all replays that are saved in the database
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<AdvancedReplay>> LoadReplaysAsync(IEnumerable<string> ids, CancellationToken cancellationToken, ProgressState progressState = null);

        /// <summary>
        /// Inserts one replay into the database
        /// </summary>
        /// <param name="replay"></param>
        void SaveReplayAsync(AdvancedReplay replay);
    }
}
