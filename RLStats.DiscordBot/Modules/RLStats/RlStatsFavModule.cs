using Discord;
using Discord.Commands;

using Discord_Bot.Configuration;
using Discord_Bot.RLStats;

using Microsoft.Extensions.Logging;
using RLStats_Classes.AverageModels;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Discord_Bot.Modules.RLStats
{
    [Remarks("HelperModule")]
    [Name("Favorite Commands")]
    public class RlStatsFavModule : RlStatsModuleBase
    {
        private ConfigHandler<UserFavorite> _userFavoritesConfigHandler;
        public RlStatsFavModule(ILogger<RlStatsFavModule> logger, string ballchasingToken) : base(logger, ballchasingToken)
        {
            _userFavoritesConfigHandler = new ConfigHandler<UserFavorite>(Constants.UserFavoritesConfigFilePath);
        }

        [Command("fav compare")]
        [Summary("Compares your favorite stats of different times.")]
        public async Task Compare(string time, string together, params string[] names)
        {
            await CompareAndSend<AveragePlayerCore>(time, names, ConvertTogetherToBool(together));
        }

        [Command("fav today")]
        [Summary("Gets your favorite stats for one or more players for today.")]
        public async Task StatsToday(string together, params string[] names)
        {
            var averages = await CommonMethods.GetAverageRocketLeagueStats(names, new Tuple<DateTime, DateTime>(DateTime.Today, DateTime.Today + new TimeSpan(1, 0, 0, 0)), playedTogether: ConvertTogetherToBool(together));
            await OutputEpicAsync<AveragePlayerCore>(averages);
        }

        [Command("fav all")]
        [Summary("Gets your favorite stats for one or more players.")]
        public async Task StatsAllTime(string together, params string[] names)
        {
            var averages = await CommonMethods.GetAverageRocketLeagueStats(names, playedTogether: ConvertTogetherToBool(together));
            await OutputEpicAsync<AveragePlayerCore>(averages);
        }

        [Command("fav last")]
        [Summary("Gets your favorite stats for one or more players for the last [count] games.")]
        public async Task StatsLast(int count, string together, params string[] names)
        {
            var averages = await CommonMethods.GetAverageRocketLeagueStats(names, replayCap: count, playedTogether: ConvertTogetherToBool(together));
            await OutputEpicAsync<AveragePlayerCore>(averages);
        }

        [Command("set favorites")]
        [Alias("set fav")]
        [Summary("Sets and saves your favorite stats.")]
        public async Task SetFavoriteStats(params int[] statIds)
        {
            //ConfigHandler.Config
        }


        [Command("show stats list")]
        [Alias("show statlist")]
        [Summary("Shows a list of possible stats with their ids.")]
        public async Task ShowAllAvailableStatPropertiesAsync()
        {
            var tempFileName = RLStatsCommonMethods.GetAllAvailableStatPropertiesFilePath();
            await Context.Channel.SendFileAsync(new FileAttachment(tempFileName, "allProperties.json"), "All properties");
            File.Delete(tempFileName);
        }
    }
}
