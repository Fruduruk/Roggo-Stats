using Discord.Commands;
using Microsoft.Extensions.Logging;
using RLStats_Classes.AverageModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Modules.RLStats
{
    public class RlStatsPositioningModule : RlStatsModuleBase
    {
        public RlStatsPositioningModule(ILogger<RlStatsModuleBase> logger, string ballchasingToken) : base(logger, ballchasingToken)
        {
        }

        [Command("pos compare")]
        public async Task Compare(string time, string together, params string[] names)
        {
            await Compare<AveragePlayerPositioning>(time, names, playedTogether: ConvertTogetherToBool(together));
        }

        [Command("pos today")]
        public async Task PositioningToday(string together, params string[] names)
        {
            var averages = await AverageStatsForTime(names, new Tuple<DateTime, DateTime>(DateTime.Today, DateTime.Today + new TimeSpan(1, 0, 0, 0)), playedTogether: ConvertTogetherToBool(together));
            await OutputEpicAsync<AveragePlayerPositioning>(averages);
        }

        [Command("pos all")]
        public async Task PositioningAllTime(string together, params string[] names)
        {
            var averages = await AverageStatsForTime(names, playedTogether: ConvertTogetherToBool(together));
            await OutputEpicAsync<AveragePlayerPositioning>(averages);
        }

        [Command("pos last")]
        public async Task PositioningLast(int count, string together, params string[] names)
        {
            var averages = await AverageStatsForTime(names, replayCap: count, playedTogether: ConvertTogetherToBool(together));
            await OutputEpicAsync<AveragePlayerPositioning>(averages);
        }
    }
}
