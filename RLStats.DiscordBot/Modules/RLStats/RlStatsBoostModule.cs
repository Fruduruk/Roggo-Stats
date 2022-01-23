using Discord.Commands;

using Microsoft.Extensions.Logging;

using RLStats_Classes.AverageModels;

using System;
using System.Threading.Tasks;

namespace Discord_Bot.Modules.RLStats
{
    public class RlStatsBoostModule : RlStatsModuleBase
    {
        public RlStatsBoostModule(ILogger<RlStatsBoostModule> logger, string ballchasingToken) : base(logger, ballchasingToken)
        {
        }

        [Command("boost compare")]
        public async Task Compare(string time, string together, params string[] names)
        {
            await CompareAndSend<AveragePlayerBoost>(time, names, playedTogether: ConvertTogetherToBool(together));
        }

        [Command("boost today")]
        public async Task BoostToday(string together, params string[] names)
        {
            var averages = await CommonMethods.AverageStatsForTime(names, new Tuple<DateTime, DateTime>(DateTime.Today, DateTime.Today + new TimeSpan(1, 0, 0, 0)), playedTogether: ConvertTogetherToBool(together));
            await OutputEpicAsync<AveragePlayerBoost>(averages);
        }

        [Command("boost all")]
        public async Task BoostAllTime(string together, params string[] names)
        {
            var averages = await CommonMethods.AverageStatsForTime(names, playedTogether: ConvertTogetherToBool(together));
            await OutputEpicAsync<AveragePlayerBoost>(averages);
        }

        [Command("boost last")]
        public async Task BoostLast(int count, string together, params string[] names)
        {
            var averages = await CommonMethods.AverageStatsForTime(names, replayCap: count, playedTogether: ConvertTogetherToBool(together));
            await OutputEpicAsync<AveragePlayerBoost>(averages);
        }
    }
}
