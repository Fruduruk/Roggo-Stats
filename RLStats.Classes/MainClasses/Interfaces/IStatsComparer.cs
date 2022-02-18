using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RLStats_Classes.Models.Advanced;
using RLStats_Classes.Models.Average;
using RLStats_Classes.Models.Chart;

namespace RLStats_Classes.MainClasses.Interfaces
{
    public interface IStatsComparer
    {
        Task<IEnumerable<AveragePlayerStats>> GetAveragesAsync(IEnumerable<AdvancedReplay> advancedReplays, IEnumerable<string> names);
        IEnumerable<WinratePack> CalculateMapWinRates(IEnumerable<AdvancedReplay> advancedReplays, string nameOrSteamId);
        IEnumerable<WinratePack> CalculateWeekDayWinrates(IEnumerable<AdvancedReplay> advancedReplays, string nameOrSteamId);
    }
}
