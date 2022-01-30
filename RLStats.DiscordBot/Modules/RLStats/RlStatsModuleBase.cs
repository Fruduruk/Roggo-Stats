using Discord;
using Discord.Commands;
using Discord.Rest;

using Discord_Bot.ExtensionMethods;
using Discord_Bot.RLStats;

using Microsoft.Extensions.Logging;

using RLStats_Classes.AdvancedModels;
using RLStats_Classes.AverageModels;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.Modules.RLStats
{
    public class RlStatsModuleBase : ModuleBase<SocketCommandContext>
    {
        private readonly RLStatsCommonMethods _commonMethods;

        protected ILogger<RlStatsModuleBase> Logger { get; }
        protected RLStatsCommonMethods CommonMethods => GetCommmonMethods();

        private RLStatsCommonMethods GetCommmonMethods()
        {
            Context.Channel.TriggerTypingAsync();
            return _commonMethods;
        }

        public RlStatsModuleBase(ILogger<RlStatsModuleBase> logger, string ballchasingToken)
        {
            Logger = logger;
            _commonMethods = new RLStatsCommonMethods(logger, ballchasingToken);
        }

        protected bool ConvertTogetherToBool(string together)
        {
            if (together.ToLower().Equals("y"))
                return true;
            else if (together.ToLower().Equals("n"))
                return false;
            else
            {
                throw new ArgumentOutOfRangeException($"{together} is not a valid together parameter. Use y or n");
            }
        }

        protected async Task OutputEpicAsync<T>(IEnumerable<AveragePlayerStats> averages)
        {
            var pathList = CommonMethods.CreateAndGetStatsFiles<T>(averages);
            await SendFilesAsync(pathList);
        }

        protected async Task OutputEpicAsync(IEnumerable<AveragePlayerStats> averages, IEnumerable<string> propertyNames)
        {
            var pathList = CommonMethods.CreateAndGetStatsFiles(averages, propertyNames);
            await SendFilesAsync(pathList);
        }

        protected async Task<RestUserMessage> SendMessageToCurrentChannelAsync(string message)
        {
            return await Context.Channel.SendMessageAsync(message);
        }

        private async Task SendFilesAsync(IEnumerable<string> pathList)
        {
            foreach (var filePath in pathList)
            {
                await Context.Channel.SendFileAsync(filePath);
                if (File.Exists(filePath))
                    File.Delete(filePath);
                await Task.Delay(1337);
            }
        }

        protected async Task OutputEpicAsync<T>(IEnumerable<AveragePlayerStats> averages,
            IEnumerable<AveragePlayerStats> averagesToCompare)
        {
            var pathList = CommonMethods.CreateAndGetStatsFiles<T>(averages, averagesToCompare);
            await SendFilesAsync(pathList);
        }

        protected async Task OutputEpicAsync(IEnumerable<AveragePlayerStats> averages,
            IEnumerable<AveragePlayerStats> averagesToCompare, IEnumerable<string> propertyNames)
        {
            var pathList = CommonMethods.CreateAndGetStatsFiles(averages, averagesToCompare, propertyNames);
            await SendFilesAsync(pathList);
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

        protected async Task CompareAndSend<T>(string time, string[] names, bool playedTogether = true)
        {
            try
            {
                var (stats, statsToCompare) = await CommonMethods.Compare(time, names, playedTogether);
                await OutputEpicAsync<T>(statsToCompare, stats);
            }
            catch (ArgumentOutOfRangeException e)
            {
                await Context.Channel.SendMessageAsync(e.Message);
            }
        }

        protected async Task CompareAndSend(string time, string[] names, IEnumerable<string> propertyNames, bool playedTogether = true)
        {
            try
            {
                var (stats, statsToCompare) = await CommonMethods.Compare(time, names, playedTogether);
                await OutputEpicAsync(statsToCompare, stats, propertyNames);
            }
            catch (ArgumentOutOfRangeException e)
            {
                await Context.Channel.SendMessageAsync(e.Message);
            }
        }

        protected async Task<bool> CheckParameters(string together, string time = null, params string[] names)
        {
            var paramsCorrect = true;
            var builder = new StringBuilder("False parameters:");
            try
            {
                if (names is null || names.Length == 0)
                {
                    throw new ArgumentOutOfRangeException("No name was given");
                }
            }
            catch (ArgumentOutOfRangeException e)
            {
                builder.Append('\n')
                    .Append(e.Message);
                paramsCorrect = false;
            }
            try
            {
                _ = ConvertTogetherToBool(together);
            }
            catch (ArgumentOutOfRangeException e)
            {
                builder.Append('\n')
                    .Append(e.Message);
                paramsCorrect = false;
            }
            if (time != null)
            {
                try
                {
                    _ = time.ConvertToThisTimeRange();
                }
                catch (ArgumentOutOfRangeException e)
                {
                    builder.Append('\n')
                        .Append(e.Message);
                    paramsCorrect = false;
                }
            }
            if (!paramsCorrect)
                await Context.Channel.SendMessageAsync(builder.ToString());
            return paramsCorrect;
        }
    }

}
