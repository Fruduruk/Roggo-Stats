using Discord.Commands;

using Microsoft.Extensions.Logging;

using RLStatsClasses.Interfaces;
using RLStatsClasses.Models.ReplayModels.Average;

using System;
using System.Threading.Tasks;

namespace Discord_Bot.Modules.RLStats
{
    public class RlStatsDemoModule : RlStatsModuleBase
    {
        public RlStatsDemoModule(ILogger<RlStatsDemoModule> logger, IDatabase database, IReplayCache replayCache, string ballchasingToken) : base(logger, database, replayCache, ballchasingToken)
        {
        }

        [Command("demo compare")]
        public async Task Compare(string time, string together, params string[] names)
        {
            try
            {
                await CompareAndSend<AveragePlayerDemo>(time, names, playedTogether: ConvertTogetherToBool(together));
            }
            catch (Exception ex)
            {
                await SendMessageToCurrentChannelAsync(ex.Message);
            }
        }

        [Command("demo today")]
        public async Task DemoToday(string together, params string[] names)
        {
            try
            {
                var time = new Tuple<DateTime, DateTime>(DateTime.Today, DateTime.Today + new TimeSpan(1, 0, 0, 0));
                var averages = await CommonMethods.GetAverageRocketLeagueStats(names, time, playedTogether: ConvertTogetherToBool(together));
                await OutputEpicAsync<AveragePlayerDemo>(averages);
            }
            catch (Exception ex)
            {
                await SendMessageToCurrentChannelAsync(ex.Message);
            }
        }

        [Command("demo all")]
        public async Task DemoAllTime(string together, params string[] names)
        {
            try
            {
                var averages = await CommonMethods.GetAverageRocketLeagueStats(names, playedTogether: ConvertTogetherToBool(together));
                await OutputEpicAsync<AveragePlayerDemo>(averages);
            }
            catch (Exception ex)
            {
                await SendMessageToCurrentChannelAsync(ex.Message);
            }
        }

        [Command("demo last")]
        public async Task DemoLast(int count, string together, params string[] names)
        {
            try
            {
                var averages = await CommonMethods.GetAverageRocketLeagueStats(names, replayCap: count, playedTogether: ConvertTogetherToBool(together));
                await OutputEpicAsync<AveragePlayerDemo>(averages);
            }
            catch (Exception ex)
            {
                await SendMessageToCurrentChannelAsync(ex.Message);
            }
        }
    }
}
