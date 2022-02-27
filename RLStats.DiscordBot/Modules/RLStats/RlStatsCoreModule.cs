using Discord.Commands;

using Microsoft.Extensions.Logging;

using RLStatsClasses.Interfaces;
using RLStatsClasses.Models.ReplayModels.Average;

using System;
using System.Threading.Tasks;

namespace Discord_Bot.Modules.RLStats
{
    [Remarks("HelperModule")]
    [Name("Core Commands")]
    public class RlStatsCoreModule : RlStatsModuleBase
    {
        public RlStatsCoreModule(ILogger<RlStatsCoreModule> logger, IDatabase database, IReplayCache replayCache, string ballchasingToken) : base(logger, database, replayCache, ballchasingToken)
        {
        }

        [Command("stats compare")]
        [Summary("Compares core stats of different times.")]
        public async Task Compare(string time, string together, params string[] names)
        {
            try
            {
                await CompareAndSend<AveragePlayerCore>(time, names, ConvertTogetherToBool(together));
            }
            catch (Exception ex)
            {
                await SendMessageToCurrentChannelAsync(ex.Message);
            }
        }

        [Command("stats today")]
        [Summary("Gets the average core stats for one or more players for today.")]
        public async Task StatsToday(string together, params string[] names)
        {
            try
            {
                var time = new Tuple<DateTime, DateTime>(DateTime.Today, DateTime.Today + new TimeSpan(1, 0, 0, 0));
                var averages = await CommonMethods.GetAverageRocketLeagueStats(names, time, playedTogether: ConvertTogetherToBool(together));
                await OutputEpicAsync<AveragePlayerCore>(averages);
            }
            catch (Exception ex)
            {
                await SendMessageToCurrentChannelAsync(ex.Message);
            }
        }

        [Command("stats all")]
        [Summary("Gets the average core stats for one or more players.")]
        public async Task StatsAllTime(string together, params string[] names)
        {
            try
            {
                var averages = await CommonMethods.GetAverageRocketLeagueStats(names, playedTogether: ConvertTogetherToBool(together));
                await OutputEpicAsync<AveragePlayerCore>(averages);
            }
            catch (Exception ex)
            {
                await SendMessageToCurrentChannelAsync(ex.Message);
            }
        }

        [Command("stats last")]
        [Summary("Gets the average core stats for one or more players for the last [count] games.")]
        public async Task StatsLast(int count, string together, params string[] names)
        {
            try
            {
                var averages = await CommonMethods.GetAverageRocketLeagueStats(names, replayCap: count, playedTogether: ConvertTogetherToBool(together));
                await OutputEpicAsync<AveragePlayerCore>(averages);
            }
            catch (Exception ex)
            {
                await SendMessageToCurrentChannelAsync(ex.Message);
            }
        }
    }
}
