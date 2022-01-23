using Discord.Commands;
using Microsoft.Extensions.Logging;
using RLStats_Classes.AverageModels;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Modules.RLStats
{
    [Remarks("HelperModule")]
    [Name("Core Commands")]
    public class RlStatsCoreModule : RlStatsModuleBase
    {
        public RlStatsCoreModule(ILogger<RlStatsCoreModule> logger, string ballchasingToken) : base(logger, ballchasingToken)
        {
        }

        [Command("stats compare")]
        [Summary("Compares core stats of different times.")]
        public async Task Compare(string time, string together, params string[] names)
        {
            await CompareAndSend<AveragePlayerCore>(time, names, ConvertTogetherToBool(together));
        }

        [Command("stats today")]
        [Summary("Gets the average core stats for one or more players for today.")]
        public async Task StatsToday(string together, params string[] names)
        {
            var averages = await CommonMethods.AverageStatsForTime(names, new Tuple<DateTime, DateTime>(DateTime.Today, DateTime.Today + new TimeSpan(1, 0, 0, 0)), playedTogether: ConvertTogetherToBool(together));
            await OutputEpicAsync<AveragePlayerCore>(averages);
        }

        [Command("stats all")]
        [Summary("Gets the average core stats for one or more players.")]
        public async Task StatsAllTime(string together, params string[] names)
        {
            var averages = await CommonMethods.AverageStatsForTime(names, playedTogether: ConvertTogetherToBool(together));
            await OutputEpicAsync<AveragePlayerCore>(averages);
        }

        [Command("stats last")]
        [Summary("Gets the average core stats for one or more players for the last [count] games.")]
        public async Task StatsLast(int count, string together, params string[] names)
        {
            var averages = await CommonMethods.AverageStatsForTime(names, replayCap: count, playedTogether: ConvertTogetherToBool(together));
            await OutputEpicAsync<AveragePlayerCore>(averages);
        }
    }
}
