using Discord.Commands;

using Microsoft.Extensions.Logging;

using RLStatsClasses.Interfaces;
using RLStatsClasses.Models.ReplayModels.Average;

using System;
using System.Threading.Tasks;

namespace Discord_Bot.Modules.RLStats
{
    public class RlStatsBoostModule : RlStatsModuleBase
    {
        public RlStatsBoostModule(ILogger<RlStatsBoostModule> logger, IDatabase database, IReplayCache replayCache, string ballchasingToken) : base(logger, database, replayCache, ballchasingToken)
        {
        }

        [Command("boost compare")]
        public async Task Compare(string time, string together, params string[] names)
        {
            try
            {
                await CompareAndSend<AveragePlayerBoost>(time, names, playedTogether: ConvertBoolenStringToBool(together));
            }
            catch (Exception ex)
            {
                await SendMessageToCurrentChannelAsync(ex.Message);
            }
        }

        [Command("boost today")]
        public async Task BoostToday(string together, params string[] names)
        {
            try
            {
                var time = new Tuple<DateTime, DateTime>(DateTime.Today, DateTime.Today + new TimeSpan(1, 0, 0, 0));
                var averages = await CommonMethods.GetAverageRocketLeagueStats(names, time, playedTogether: ConvertBoolenStringToBool(together));
                await OutputEpicAsync<AveragePlayerBoost>(averages);
            }
            catch (Exception ex)
            {
                await SendMessageToCurrentChannelAsync(ex.Message);
            }
        }

        [Command("boost all")]
        public async Task BoostAllTime(string together, params string[] names)
        {
            try
            {
                var averages = await CommonMethods.GetAverageRocketLeagueStats(names, playedTogether: ConvertBoolenStringToBool(together));
                await OutputEpicAsync<AveragePlayerBoost>(averages);
            }
            catch (Exception ex)
            {
                await SendMessageToCurrentChannelAsync(ex.Message);
            }
        }

        [Command("boost last")]
        public async Task BoostLast(int count, string together, params string[] names)
        {
            try
            {
                var averages = await CommonMethods.GetAverageRocketLeagueStats(names, replayCap: count, playedTogether: ConvertBoolenStringToBool(together));
                await OutputEpicAsync<AveragePlayerBoost>(averages);
            }
            catch (Exception ex)
            {
                await SendMessageToCurrentChannelAsync(ex.Message);
            }
        }
    }
}
