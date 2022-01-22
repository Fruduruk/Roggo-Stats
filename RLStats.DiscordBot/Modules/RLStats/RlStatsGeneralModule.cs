using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using RLStats_Classes.MainClasses;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Modules.RLStats
{
    //[Remarks("HelperModule")]
    [Name("General Commands")]
    public class RlStatsGeneralModule : RlStatsModuleBase
    {
        public RlStatsGeneralModule(ILogger<RlStatsModuleBase> logger, string ballchasingToken) : base(logger, ballchasingToken)
        {
        }

        [Command("weekday winrates")]
        [Summary("Gets the weekday winrates for the last 7 days.")]
        public async Task Winrate(params string[] names)
        {
            ConvertForDanschl(names);
            var dMessage = await Context.Channel.SendMessageAsync("Please wait, this might take a while.");
            var filter = new APIRequestFilter
            {
                CheckDate = true,
                DateRange = new Tuple<DateTime, DateTime>(DateTime.Today.Subtract(TimeSpan.FromDays(6)), DateTime.Today)
            };
            AddNameOrSteamIds(names, filter);
            await dMessage.ModifyAsync(props => props.Content = "Downloading normal replays...");

            var response = await ReplayProvider.CollectReplaysAsync(filter);
            var replays = response.Replays;
            await dMessage.ModifyAsync(props => props.Content = "Downloading advanced replay stats...");
            var advancedReplays = await AdvancedReplayProvider.GetAdvancedReplayInfosAsync(replays.ToList());
            await dMessage.ModifyAsync(props => props.Content = $"Downloaded { replays.Count()} replays!");
            var winratePacks = StatsComparer.CalculateWeekDayWinrates(advancedReplays, names[0]);
            var builder = new EmbedBuilder();
            builder.WithTitle($"Weekday winrates for {string.Join(", ", names)}");
            foreach (var p in winratePacks)
            {
                if (!p.Played.Equals(0))
                {
                    var text = $"{p.WinrateString}\t{p.Won} wins\t{p.Played} games";
                    builder.AddField(p.Name, text);
                }
                else
                {
                    builder.AddField(p.Name, "no games played");
                }

            }
            await Context.Channel.SendMessageAsync(null, false, builder.Build());
        }
    }
}
