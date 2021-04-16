using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RLStats_Classes.AdvancedModels;
using RLStats_Classes.AverageModels;

namespace RLStats_Classes.MainClasses.Interfaces
{
    public interface IStatsComparer
    {
        Task<Dictionary<string, AveragePlayerStats>> GetAveragesAsync(IEnumerable<AdvancedReplay> advancedReplays, IEnumerable<string> names);
        IEnumerable CalculateMapWinRates(IEnumerable<AdvancedReplay> advancedReplays, string nameOrSteamId);
        IEnumerable CalculateWeekDayWinrates(IEnumerable<AdvancedReplay> advancedReplays, string nameOrSteamId);
    }
}
