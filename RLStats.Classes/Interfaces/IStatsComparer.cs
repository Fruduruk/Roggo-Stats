using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RLStatsClasses.Models.Chart;
using RLStatsClasses.Models.ReplayModels.Advanced;
using RLStatsClasses.Models.ReplayModels.Average;

namespace RLStatsClasses.Interfaces
{
    public interface IStatsComparer
    {
        Task<IEnumerable<AveragePlayerStats>> GetAveragesAsync(IEnumerable<AdvancedReplay> advancedReplays, IEnumerable<string> names);
        IEnumerable<WinratePack> CalculateMapWinRates(IEnumerable<AdvancedReplay> advancedReplays, string nameOrSteamId);
        IEnumerable<WinratePack> CalculateWeekDayWinrates(IEnumerable<AdvancedReplay> advancedReplays, string nameOrSteamId);
    }
}
