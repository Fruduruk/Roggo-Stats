using Discord;

using Discord_Bot.ExtensionMethods;
using Discord_Bot.Modules.RLStats;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using RLStats_Classes.AverageModels;
using RLStats_Classes.MainClasses;
using RLStats_Classes.MainClasses.Interfaces;

using RLStats_WPF;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Discord_Bot.RLStats
{
    public class RLStatsCommonMethods
    {
        protected ILogger Logger { get; }
        private IReplayProvider ReplayProvider { get; }
        private IAdvancedReplayProvider AdvancedReplayProvider { get; }
        private IStatsComparer StatsComparer { get; }
        public RLStatsCommonMethods(ILogger logger, string ballchasingToken)
        {
            var tokeninfo = TokenInfoProvider.GetTokenInfo(ballchasingToken);
            ReplayProvider = new ReplayProvider(tokeninfo);
            AdvancedReplayProvider = new AdvancedReplayProvider(tokeninfo);
            StatsComparer = new StatsComparer();
            Logger = logger;
        }
        private void AddNameOrSteamIds(string[] namesOrIds, APIRequestFilter filter)
        {
            foreach (var nameOrId in namesOrIds)
            {
                if (IsNumber(nameOrId))
                {
                    filter.CheckSteamId = true;
                    if (filter.SteamIDs is null)
                        filter.SteamIDs = new List<string>();
                    filter.SteamIDs.Add(nameOrId);
                }
                else
                {
                    filter.CheckName = true;
                    if (filter.Names is null)
                        filter.Names = new List<string>();
                    filter.Names.Add(nameOrId);
                }
            }

            bool IsNumber(string text)
            {
                foreach (var c in text)
                {
                    if (!char.IsNumber(c))
                        return false;
                }
                return true;
            }
        }
        /// <summary>
        /// This is the most important method in this whole program.
        /// It downloads average stats for specific players.
        /// </summary>
        /// <param name="nameOrSteamIds">There has to be at least one player name or steam id</param>
        /// <param name="dateRange">This method downloads every replay inside this range. Unless it is null, then it downloads all.</param>
        /// <param name="replayCap">This marks the cap at which no more replays are downloaded.</param>
        /// <param name="playedTogether">This is true if you want the average stats for the games the players played together. If you don't care if they played together then make this false. Caution: False will take longer to load.</param>
        /// <returns></returns>
        public async Task<IEnumerable<AveragePlayerStats>> GetAverageRocketLeagueStats(
            string[] nameOrSteamIds,
            Tuple<DateTime, DateTime> dateRange = null,
            int replayCap = 0,
            bool playedTogether = true)
        {
            if (nameOrSteamIds is null || nameOrSteamIds.Length == 0)
            {
                throw new Exception("No name was given");
            }
            List<AveragePlayerStats> averages = new List<AveragePlayerStats>();
            ConvertForDanschl(nameOrSteamIds);

            if (playedTogether)
                averages.AddRange(await GetAverages(nameOrSteamIds));
            else
                foreach (var nameOrId in nameOrSteamIds)
                {
                    averages.AddRange(await GetAverages(new string[] { nameOrId }));
                }

            averages.ConvertDansSteamIdToName();

            return averages;

            async Task<IEnumerable<AveragePlayerStats>> GetAverages(IEnumerable<string> namesToUse)
            {
                var filter = new APIRequestFilter
                {
                    CheckName = true,
                    CheckDate = dateRange is not null,
                    DateRange = dateRange,
                    ReplayCap = replayCap
                };
                AddNameOrSteamIds(namesToUse.ToArray(), filter);

                //var dMessage = await Context.Channel.SendMessageAsync($"Downloading replays for {string.Join(',', namesToUse)}. Please wait.");

                var response = await ReplayProvider.CollectReplaysAsync(filter, false);

                var advancedReplays = await AdvancedReplayProvider.GetAdvancedReplayInfosAsync(response.Replays.ToList());

                //await dMessage.ModifyAsync(props => props.Content = $"Downloaded { response.Replays.Count()} replays for {string.Join(',', namesToUse)}!");
                return await StatsComparer.GetAveragesAsync(advancedReplays, new List<string>(namesToUse));
            }
        }

        public async Task<(IEnumerable<AveragePlayerStats>, IEnumerable<AveragePlayerStats>)> Compare<T>(string time, string[] names, bool playedTogether = true)
        {
            var startTimeRange = time.ConvertToThisTimeRange();
            var endTimeRange = time.ConvertToLastTimeRange();

            var stats = await GetAverageStatsForTimeRange(names, startTimeRange, playedTogether);

            var statsToCompare = await GetAverageStatsForTimeRange(names, endTimeRange, playedTogether);

            return (stats, statsToCompare);
        }

        private async Task<IEnumerable<AveragePlayerStats>> GetAverageStatsForTimeRange(IEnumerable<string> names, TimeRange timeRange, bool playedTogether)
        {
            var averages = new List<AveragePlayerStats>();
            var dateRange = timeRange.ConvertToDateTimeRange();
            averages.AddRange(await GetAverageRocketLeagueStats(names.ToArray(), dateRange, playedTogether: playedTogether));

            return averages;
        }

        private static void ConvertForDanschl(string[] nameOrSteamIds)
        {
            for (var i = 0; i < nameOrSteamIds.Length; i++)
            {
                var nameOrId = nameOrSteamIds[i];
                if (nameOrId.ToLower().Equals("danschl"))
                    nameOrSteamIds[i] = "76561198095673686";
            }
        }

        private string GetPath(IEnumerable<ChartCreator> chartCreators, int maxColumns)
        {
            var merger = new ChartMerger(chartCreators);
            string filePath = null;
            var thread = new Thread(() =>
            {
                try
                {
                    filePath = merger.CreatePngImageAsStream(GetRlStatsTempFolder() + Guid.NewGuid(), maxColumns);
                }
                catch (Exception ex)
                {
                    Logger.LogCritical(ex.Message);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (filePath is not null)
            {
                return filePath;
            }

            return filePath;
        }

        public static string GetRlStatsTempFolder()
        {
            var tempPath = Path.GetTempPath();
            var info = Directory.CreateDirectory(tempPath + @"rlCharts\");
            return info.FullName;
        }

        public static string GetAllAvailableStatPropertiesFilePath()
        {
            var dic = GetPropertyNameDictionary();
            var tempFileName = Path.Combine(GetRlStatsTempFolder(), "AllProperties.json");
            File.WriteAllText(tempFileName, JsonConvert.SerializeObject(dic, Formatting.Indented));
            return tempFileName;
        }

        private static Dictionary<int, string> GetPropertyNameDictionary()
        {
            var properties = AveragePlayerStats.GetAllPropertyNames();
            var dic = new Dictionary<int, string>();
            for (int i = 0; i < properties.Length; i++)
                dic.Add(i, properties[i].Replace('_', ' '));
            return dic;
        }

        private IEnumerable<string> GetPathList(IEnumerable<ChartCreator> chartCreators, int groupSize, int maxColumns)
        {
            var pathList = new List<string>();
            var creatorGroup = new List<ChartCreator>();
            foreach (var creator in chartCreators)
            {
                creatorGroup.Add(creator);
                if (creatorGroup.Count.Equals(groupSize))
                {
                    pathList.Add(GetPath(creatorGroup, maxColumns));
                    creatorGroup.Clear();
                }
            }
            if (creatorGroup.Count > 0)
                pathList.Add(GetPath(creatorGroup, maxColumns));
            return pathList;
        }

        public IEnumerable<string> CreateAndGetStatsFiles<T>(IEnumerable<AveragePlayerStats> averages)
        {
            var provider = new ChartProvider(averages);
            var chartCreators = provider.GetChartCreators<T>(350, 600);
            var pathList = GetPathList(chartCreators, 9, 3);
            return pathList;
        }

        public IEnumerable<string> CreateAndGetStatsFiles<T>(IEnumerable<AveragePlayerStats> averages, IEnumerable<AveragePlayerStats> averagesToCompare)
        {
            var provider = new ChartProvider(averages, averagesToCompare);
            var chartCreators = provider.GetChartCreators<T>(350, 600);
            var pathList = GetPathList(chartCreators, 9, 3);
            return pathList;
        }
        public IEnumerable<string> CreateAndGetStatsFiles(IEnumerable<AveragePlayerStats> averages, IEnumerable<string> propertyNames)
        {
            var provider = new ChartProvider(averages);
            var chartCreators = provider.GetSpecificCreators(propertyNames, 350, 600);
            var pathList = GetPathList(chartCreators, 4, 2);
            return pathList;
        }

        public IEnumerable<string> CreateAndGetStatsFiles(IEnumerable<AveragePlayerStats> averages, IEnumerable<AveragePlayerStats> averagesToCompare, IEnumerable<string> propertyNames)
        {
            var provider = new ChartProvider(averages, averagesToCompare);
            var chartCreators = provider.GetSpecificCreators(propertyNames, 350, 600);
            var pathList = GetPathList(chartCreators, 4, 2);
            return pathList;
        }
    }
}
