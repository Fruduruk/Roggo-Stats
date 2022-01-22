using Discord.Commands;
using Microsoft.Extensions.Logging;
using RLStats_Classes.AverageModels;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Modules.RLStats
{
    public class RlStatsDemoModule : RlStatsModuleBase
    {
        public RlStatsDemoModule(ILogger<RlStatsModuleBase> logger, string ballchasingToken) : base(logger, ballchasingToken)
        {
        }

        [Command("demo compare")]
        public async Task Compare(string time, string together, params string[] names)
        {
            await Compare<AveragePlayerDemo>(time, names, playedTogether: ConvertTogetherToBool(together));
        }

        [Command("demo today")]
        public async Task DemoToday(string together, params string[] names)
        {
            var averages = await AverageStatsForTime(names, new Tuple<DateTime, DateTime>(DateTime.Today, DateTime.Today + new TimeSpan(1, 0, 0, 0)), playedTogether: ConvertTogetherToBool(together));
            await OutputEpicAsync<AveragePlayerDemo>(averages);
        }

        [Command("demo all")]
        public async Task DemoAllTime(string together, params string[] names)
        {
            var averages = await AverageStatsForTime(names, playedTogether: ConvertTogetherToBool(together));
            await OutputEpicAsync<AveragePlayerDemo>(averages);
        }

        [Command("demo last")]
        public async Task DemoLast(int count,  string together, params string[] names)
        {
            var averages = await AverageStatsForTime(names, replayCap: count, playedTogether: ConvertTogetherToBool(together));
            await OutputEpicAsync<AveragePlayerDemo>(averages);
        }
    }
}
