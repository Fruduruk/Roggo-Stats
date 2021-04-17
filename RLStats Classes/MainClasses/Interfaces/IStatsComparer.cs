using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RLStats_Classes.AdvancedModels;
using RLStats_Classes.AverageModels;
using RLStats_Classes.ChartModels;

namespace RLStats_Classes.MainClasses.Interfaces
{
    public interface IStatsComparer
    {
        Task<IEnumerable<AveragePlayerStats>> GetAveragesAsync(IEnumerable<AdvancedReplay> advancedReplays, IEnumerable<string> names);
        IEnumerable<WinratePack> CalculateMapWinRates(IEnumerable<AdvancedReplay> advancedReplays, string nameOrSteamId);
        IEnumerable<WinratePack> CalculateWeekDayWinrates(IEnumerable<AdvancedReplay> advancedReplays, string nameOrSteamId);
    }
}
