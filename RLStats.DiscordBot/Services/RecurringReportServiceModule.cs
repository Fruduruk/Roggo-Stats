﻿using Discord_Bot.RLStats;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using RLStats_Classes.AverageModels;
using System.Threading.Tasks;
using Discord_Bot.ExtensionMethods;
using System;
using System.Linq;
using Discord_Bot.Configuration;

namespace Discord_Bot.Services
{
    public class RecurringReportServiceModule
    {
        private RLStatsCommonMethods CommonMethods { get; set; }
        public RecurringReportServiceModule(ILogger logger, string ballchasingToken)
        {
            CommonMethods = new RLStatsCommonMethods(logger, ballchasingToken);
        }

        public async Task<IEnumerable<string>> GetAverageStats(SubscriptionConfigEntry entry)
        {
            var averages = await CommonMethods.GetAverageRocketLeagueStats(entry.Names.ToArray(), dateRange: entry.Time.ConvertToThisTimeRange().ConvertToDateTimeRange() ,playedTogether: entry.Together);
            var pathList = new List<string>();
            averages = RemoveEmptyAverages(averages);
            if (averages.Any())
            {
                pathList.AddRange(CommonMethods.CreateAndGetStatsFiles(averages, propertyNames: entry.StatNames));
                
                return pathList;
            }
            return Array.Empty<string>();
        }

        private IEnumerable<AveragePlayerStats> RemoveEmptyAverages(IEnumerable<AveragePlayerStats> averages)
        {
            var notEmptyAverages = new List<AveragePlayerStats>();
            foreach(var average in averages)
            {
                if(!average.IsEmpty)
                    notEmptyAverages.Add(average);
            }
            return notEmptyAverages;
        }
    }
}
