using Discord.Commands;
using Microsoft.Extensions.Logging;
using RLStats_Classes.AverageModels;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Modules.RLStats
{
    public class RlStatsMovementModule :RlStatsModuleBase
    {
        public RlStatsMovementModule(ILogger<RlStatsMovementModule> logger, string ballchasingToken) : base(logger, ballchasingToken)
        {
        }

        [Command("mov compare")]
        public async Task Compare(string time, string together, params string[] names)
        {
            await CompareAndSend<AveragePlayerMovement>(time, names, playedTogether: ConvertTogetherToBool(together));
        }

        [Command("mov today")]
        public async Task MovementToday(string together, params string[] names)
        {
            var averages = await CommonMethods.AverageStatsForTime(names, new Tuple<DateTime, DateTime>(DateTime.Today, DateTime.Today + new TimeSpan(1, 0, 0, 0)), playedTogether: ConvertTogetherToBool(together));
            await OutputEpicAsync<AveragePlayerMovement>(averages);
        }

        [Command("mov all")]
        public async Task MovementAllTime(string together, params string[] names)
        {
            var averages = await CommonMethods.AverageStatsForTime(names, playedTogether: ConvertTogetherToBool(together));
            await OutputEpicAsync<AveragePlayerMovement>(averages);
        }

        [Command("mov last")]
        public async Task MovementLast(int count, string together, params string[] names)
        {
            var averages = await CommonMethods.AverageStatsForTime(names, replayCap: count, playedTogether: ConvertTogetherToBool(together));
            await OutputEpicAsync<AveragePlayerMovement>(averages);
        }
    }
}
