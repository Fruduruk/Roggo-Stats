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
            try
            {
                await CompareAndSend<AveragePlayerMovement>(time, names, playedTogether: ConvertTogetherToBool(together));
            }
            catch (Exception ex)
            {
                await SendMessageToCurrentChannelAsync(ex.Message);
            }
        }

        [Command("mov today")]
        public async Task MovementToday(string together, params string[] names)
        {
            try
            {
                var time = new Tuple<DateTime, DateTime>(DateTime.Today, DateTime.Today);
                var averages = await CommonMethods.GetAverageRocketLeagueStats(names, time, playedTogether: ConvertTogetherToBool(together));
                await OutputEpicAsync<AveragePlayerMovement>(averages);
            }
            catch (Exception ex)
            {
                await SendMessageToCurrentChannelAsync(ex.Message);
            }
        }

        [Command("mov all")]
        public async Task MovementAllTime(string together, params string[] names)
        {
            try
            {
                var averages = await CommonMethods.GetAverageRocketLeagueStats(names, playedTogether: ConvertTogetherToBool(together));
                await OutputEpicAsync<AveragePlayerMovement>(averages);
            }
            catch (Exception ex)
            {
                await SendMessageToCurrentChannelAsync(ex.Message);
            }
        }

        [Command("mov last")]
        public async Task MovementLast(int count, string together, params string[] names)
        {
            try
            {
                var averages = await CommonMethods.GetAverageRocketLeagueStats(names, replayCap: count, playedTogether: ConvertTogetherToBool(together));
                await OutputEpicAsync<AveragePlayerMovement>(averages);
            }
            catch (Exception ex)
            {
                await SendMessageToCurrentChannelAsync(ex.Message);
            }
        }
    }
}
