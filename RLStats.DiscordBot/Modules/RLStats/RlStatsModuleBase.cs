using Discord;
using Discord.Commands;

using Microsoft.Extensions.Logging;

using RLStats_Classes.AdvancedModels;
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

namespace Discord_Bot.Modules.RLStats
{
    public class RlStatsModuleBase : ModuleBase<SocketCommandContext>
    {
        protected ILogger<RlStatsModuleBase> Logger { get; }
        protected IReplayProvider ReplayProvider { get; }
        protected IAdvancedReplayProvider AdvancedReplayProvider { get; }
        protected IStatsComparer StatsComparer { get; }

        public RlStatsModuleBase(ILogger<RlStatsModuleBase> logger, string ballchasingToken)
        {
            Logger = logger;
            var tokeninfo = TokenInfoProvider.GetTokenInfo(ballchasingToken);
            ReplayProvider = new ReplayProvider(tokeninfo);
            AdvancedReplayProvider = new AdvancedReplayProvider(tokeninfo);
            StatsComparer = new StatsComparer();
        }

        protected bool ConvertTogetherToBool(string together)
        {
            if (together.ToLower().Equals("y"))
                return true;
            else if (together.ToLower().Equals("n"))
                return false;
            else
            {
                throw new Exception($"{together} is not a valid together parameter. Use y or n");
            }
        }
        protected void AddNameOrSteamIds(string[] namesOrIds, APIRequestFilter filter)
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
        protected async Task<IEnumerable<AveragePlayerStats>> AverageStatsForTime(
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

                var dMessage = await Context.Channel.SendMessageAsync($"Downloading replays for {string.Join(',', namesToUse)}. Please wait.");

                var response = await ReplayProvider.CollectReplaysAsync(filter);

                var advancedReplays = await AdvancedReplayProvider.GetAdvancedReplayInfosAsync(response.Replays.ToList());

                await dMessage.ModifyAsync(props => props.Content = $"Downloaded { response.Replays.Count()} replays for {string.Join(',', namesToUse)}!");
                return await StatsComparer.GetAveragesAsync(advancedReplays, new List<string>(namesToUse));
            }
        }

        protected async Task OutputEpicAsync<T>(IEnumerable<AveragePlayerStats> averages)
        {
            var provider = new ChartProvider(averages);
            var chartCreators = provider.GetChartCreators<T>(350, 600);
            var pathList = GetPathList(chartCreators, 9, 3);
            foreach (var filePath in pathList)
            {
                await Context.Channel.SendFileAsync(filePath);
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }

        protected async Task OutputEpicAsync<T>(IEnumerable<AveragePlayerStats> averages,
            IEnumerable<AveragePlayerStats> averagesToCompare)
        {
            var provider = new ChartProvider(averages, averagesToCompare);
            var chartCreators = provider.GetChartCreators<T>(350, 600);
            var pathList = GetPathList(chartCreators, 9, 3);
            foreach (var filePath in pathList)
            {
                await Context.Channel.SendFileAsync(filePath);
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }

        protected static string GetRlStatsTempFolder()
        {
            var tempPath = Path.GetTempPath();
            var info = Directory.CreateDirectory(tempPath + @"rlCharts\");
            return info.FullName;
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

        protected async Task OutputNormal(IEnumerable<AveragePlayerStats> averages, IEnumerable<AdvancedReplay> advancedReplays)
        {
            foreach (var averagePlayerStats in averages)
            {
                int count = 0;
                foreach (var game in advancedReplays)
                    if (game.Contains(averagePlayerStats.PlayerName))
                        count++;
                var core = averagePlayerStats.AverageCore;
                var boost = averagePlayerStats.AverageBoost;
                var movement = averagePlayerStats.AverageMovement;
                var positioning = averagePlayerStats.AveragePositioning;
                var demo = averagePlayerStats.AverageDemo;
                var builder = new EmbedBuilder();
                builder.WithTitle($"Averages for {averagePlayerStats.PlayerName}")
                    .AddField("MVP", Get(core.Mvp), true)
                    .AddField("Goals", Get(core.Goals), true)
                    .AddField("Assists", Get(core.Assists), true)
                    .AddField("Saves", Get(core.Saves), true)
                    .AddField("Score", Get(core.Score), true)
                    .AddField("Shooting percentage", Get(core.Shooting_percentage), true)

                    .AddField("Boost used per minute", Get(boost.Bpm), true)
                    .AddField("Boost collected per minute", Get(boost.Bcpm), true)
                    .AddField("Count big boost collected", Get(boost.Count_collected_big), true)
                    .AddField("Count small boost collected", Get(boost.Count_collected_small), true)
                    .AddField("Count big boost stolen", Get(boost.Count_stolen_big), true)
                    .AddField("Count small boost stolen", Get(boost.Count_stolen_small), true)

                    .AddField("Percent supersonic", Get(movement.Percent_supersonic_speed), true)
                    .AddField("Percent on ground", Get(movement.Percent_ground), true)
                    .AddField("Percent in low air", Get(movement.Percent_low_air), true)
                    .AddField("Percent in high air", Get(movement.Percent_high_air), true)

                    .AddField("Percent offensive half", Get(positioning.Percent_offensive_half), true)
                    .AddField("Percent defensive half", Get(positioning.Percent_defensive_half), true)

                    .AddField("Demos inflicted", Get(demo.Inflicted), true)
                    .AddField("Demos taken", Get(demo.Taken), true)


                    .WithFooter($"Appears in {count} replays");
                await Context.Channel.SendMessageAsync(null, false, builder.Build());
            }


            string Get(double? value)
            {
                if (value is null)
                    return string.Empty;
                return value?.ToString("0.##");
            }
        }

        protected async Task Compare<T>(string time, string[] names, bool playedTogether = true)
        {
            TimeRange startTimeRange;
            TimeRange endTimeRange;
            switch (time)
            {
                case "d":
                case "day":
                    startTimeRange = TimeRange.Today;
                    endTimeRange = TimeRange.Yesterday;
                    break;
                case "w":
                case "week":
                    startTimeRange = TimeRange.ThisWeek;
                    endTimeRange = TimeRange.LastWeek;
                    break;
                case "m":
                case "month":
                    startTimeRange = TimeRange.ThisMonth;
                    endTimeRange = TimeRange.LastMonth;
                    break;
                case "y":
                case "year":
                    startTimeRange = TimeRange.ThisYear;
                    endTimeRange = TimeRange.LastYear;
                    break;
                default:
                    await Context.Channel.SendMessageAsync($"{time} is not a valid time parameter. Use d,w,m or y.");
                    return;
            }

            var stats = await GetAverageStatsForTimeRange(names, startTimeRange, playedTogether);

            var statsToCompare = await GetAverageStatsForTimeRange(names, endTimeRange, playedTogether);

            await OutputEpicAsync<T>(statsToCompare, stats);
        }

        protected async Task<IEnumerable<AveragePlayerStats>> GetAverageStatsForTimeRange(IEnumerable<string> names, TimeRange timeRange, bool playedTogether)
        {
            var averages = new List<AveragePlayerStats>();
            var dateRange = ConvertTimeRangeToDateTimeRange(timeRange);
            averages.AddRange(await AverageStatsForTime(names.ToArray(), dateRange));

            return averages;
        }

        private static Tuple<DateTime, DateTime> ConvertTimeRangeToDateTimeRange(TimeRange timeRange)
        {
            switch (timeRange)
            {
                case TimeRange.Today:
                    return new Tuple<DateTime, DateTime>(
                        DateTime.Today,
                        DateTime.Today + new TimeSpan(1, 0, 0, 0)
                        );
                case TimeRange.Yesterday:
                    return new Tuple<DateTime, DateTime>(
                        DateTime.Today.Subtract(new TimeSpan(1, 0, 0, 0)),
                        DateTime.Today
                        );
                case TimeRange.ThisWeek:
                    return new Tuple<DateTime, DateTime>(
                        DateTime.Today.Subtract(new TimeSpan(7, 0, 0, 0)),
                        DateTime.Today
                    );
                case TimeRange.LastWeek:
                    return new Tuple<DateTime, DateTime>(
                        DateTime.Today.Subtract(new TimeSpan(15, 0, 0, 0)),
                        DateTime.Today.Subtract(new TimeSpan(8, 0, 0, 0))
                    );
                case TimeRange.ThisMonth:
                    return new Tuple<DateTime, DateTime>(
                        DateTime.Today.Subtract(new TimeSpan(28, 0, 0, 0)),
                        DateTime.Today
                    );
                case TimeRange.LastMonth:
                    return new Tuple<DateTime, DateTime>(
                        DateTime.Today.Subtract(new TimeSpan(57, 0, 0, 0)),
                        DateTime.Today.Subtract(new TimeSpan(29, 0, 0, 0))
                    );
                case TimeRange.ThisYear:
                    return new Tuple<DateTime, DateTime>(
                        DateTime.Today.Subtract(new TimeSpan(365, 0, 0, 0)),
                        DateTime.Today
                    );
                case TimeRange.LastYear:
                    return new Tuple<DateTime, DateTime>(
                        DateTime.Today.Subtract(new TimeSpan(731, 0, 0, 0)),
                        DateTime.Today.Subtract(new TimeSpan(366, 0, 0, 0))
                    );
                default:
                    throw new ArgumentOutOfRangeException(nameof(timeRange), timeRange, null);
            }
        }

        protected static void ConvertForDanschl(string[] nameOrSteamIds)
        {
            for (var i = 0; i < nameOrSteamIds.Length; i++)
            {
                var nameOrId = nameOrSteamIds[i];
                if (nameOrId.ToLower().Equals("danschl"))
                    nameOrSteamIds[i] = "76561198095673686";
            }
        }
    }

}
