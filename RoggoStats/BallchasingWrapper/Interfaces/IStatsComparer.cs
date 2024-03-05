using BallchasingWrapper.Models.Chart;
using BallchasingWrapper.Models.ReplayModels.Advanced;
using BallchasingWrapper.Models.ReplayModels.Average;

namespace BallchasingWrapper.Interfaces
{
    public interface IStatsComparer
    {
        Task<IEnumerable<AveragePlayerStats>> GetAveragesAsync(IEnumerable<AdvancedReplay> advancedReplays, IEnumerable<string> names);
        IEnumerable<WinratePack> CalculateMapWinRates(IEnumerable<AdvancedReplay> advancedReplays, string nameOrSteamId);
        IEnumerable<WinratePack> CalculateWeekDayWinrates(IEnumerable<AdvancedReplay> advancedReplays, string nameOrSteamId);
    }
}
