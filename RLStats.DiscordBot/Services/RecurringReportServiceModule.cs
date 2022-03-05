using Discord_Bot.Configuration;
using Discord_Bot.ExtensionMethods;
using Discord_Bot.RLStats;

using Microsoft.Extensions.Logging;

using RLStatsClasses.Interfaces;
using RLStatsClasses.Models.ReplayModels.Average;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Services
{
    public class RecurringReportServiceModule
    {
        private RLStatsCommonMethods CommonMethods { get; set; }
        public RecurringReportServiceModule(ILogger commonMethodsLogger, IDatabase database, IReplayCache replayCache, string ballchasingToken)
        {
            CommonMethods = new RLStatsCommonMethods(commonMethodsLogger, database, replayCache, ballchasingToken);
        }

        public async Task<IEnumerable<string>> GetAverageStats(Subscription entry)
        {
            if (entry.CompareToLastTime)
                return await GetPathListWithComparedStatsAsync(entry);
            return await GetPathListWithSingleStatsAsync(entry);
        }

        private async Task<IEnumerable<string>> GetPathListWithComparedStatsAsync(Subscription entry)
        {
            var (stats, statsToCompare) = await CommonMethods.Compare(entry.Time, entry.Names.ToArray(), entry.Together);
            var statsList = new List<AveragePlayerStats>(stats);
            var statsToCompareList = new List<AveragePlayerStats>(statsToCompare);
            RemoveEmptyAverages(statsList, statsToCompareList);
            if (statsToCompare.Any())
            {
                var pathList = new List<string>(CommonMethods.CreateAndGetStatsFiles(statsList, statsToCompare, entry.StatNames));
                return pathList;
            }
            return Array.Empty<string>();
        }

        private async Task<IEnumerable<string>> GetPathListWithSingleStatsAsync(Subscription entry)
        {
            var averages = new List<AveragePlayerStats>(await CommonMethods.GetAverageRocketLeagueStats(entry.Names.ToArray(), dateRange: entry.Time.ConvertToThisTimeRange().ConvertToDateTimeRange(), playedTogether: entry.Together));
            RemoveEmptyAverages(averages);
            if (averages.Any())
            {
                var pathList = new List<string>(CommonMethods.CreateAndGetStatsFiles(averages, propertyNames: entry.StatNames));
                return pathList;
            }
            return Array.Empty<string>();
        }

        private void RemoveEmptyAverages(List<AveragePlayerStats> averages)
        {
            var notEmptyAverages = new List<AveragePlayerStats>();
            foreach (var average in averages)
            {
                if (!average.IsEmpty)
                    notEmptyAverages.Add(average);
            }
            averages.Clear();
            averages.AddRange(notEmptyAverages);
        }

        private void RemoveEmptyAverages(List<AveragePlayerStats> stats, List<AveragePlayerStats> statsToCompare)
        {
            if (stats.Count != statsToCompare.Count)
                throw new ArgumentException("Cannot remove empty Averages from lists that are not equally long");
            var notEmptyStatsList = new List<AveragePlayerStats>();
            var notEmptyStatsToCompareList = new List<AveragePlayerStats>();

            foreach(var stat in stats)
            {
                var statToCompare = statsToCompare.First(s => s.PlayerName.Equals(stat.PlayerName));
                if (stat.IsEmpty && statToCompare.IsEmpty)
                    continue;   
                notEmptyStatsList.Add(stat);
                notEmptyStatsToCompareList.Add(statToCompare);
            }
            stats.Clear();
            stats.AddRange(notEmptyStatsList);
            statsToCompare.Clear();
            statsToCompare.AddRange(notEmptyStatsToCompareList);
        }
    }
}
