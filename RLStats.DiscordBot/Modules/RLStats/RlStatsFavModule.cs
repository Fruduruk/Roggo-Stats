﻿using Discord;
using Discord.Commands;

using Discord_Bot.Configuration;
using Discord_Bot.ExtensionMethods;
using Discord_Bot.RLStats;

using Microsoft.Extensions.Logging;

using RLStatsClasses.Interfaces;
using RLStatsClasses.Models.ReplayModels.Average;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.Modules.RLStats
{
    [Remarks("HelperModule")]
    [Name("Favorite Commands")]
    public class RlStatsFavModule : RlStatsModuleBase
    {
        private const string NoneConfiguredMessage = "You don't have any favorites configured.";
        private IConfigHandler<UserFavorite> _userFavoritesConfigHandler;
        public RlStatsFavModule(ILogger<RlStatsFavModule> logger, IDatabase database, IReplayCache replayCache, IConfigHandler<UserFavorite> userFavoritesConfigHandler, string ballchasingToken) : base(logger, database, replayCache, ballchasingToken)
        {
            _userFavoritesConfigHandler = userFavoritesConfigHandler;
        }

        private async Task ShowFavoriteStats(IEnumerable<AveragePlayerStats> averages)
        {
            var favorite = GetUserFavoriteByUserId(Context.User.Id);
            if (favorite is null)
            {
                await SendMessageToCurrentChannelAsync(NoneConfiguredMessage);
                return;
            }
            await OutputEpicAsync(averages, favorite.FavoriteStats);
        }

        [Remarks(Constants.IgnoreEndpoint)]
        [Command("fav compare")]
        [Summary("Compares your favorite stats of different times.")]
        public async Task Compare(string time, string together, params string[] names)
        {
            try
            {
                var favorite = GetUserFavoriteByUserId(Context.User.Id);
                if (favorite is null)
                {
                    await SendMessageToCurrentChannelAsync(NoneConfiguredMessage);
                    return;
                }
                await CompareAndSend(time, names, favorite.FavoriteStats, ConvertTogetherToBool(together));
            }
            catch (Exception ex)
            {
                await SendMessageToCurrentChannelAsync(ex.Message);
            }

        }

        [Remarks(Constants.IgnoreEndpoint)]
        [Command("fav today")]
        [Summary("Gets your favorite stats for one or more players for today.")]
        public async Task StatsToday(string together, params string[] names)
        {
            try
            {
                var time = new Tuple<DateTime, DateTime>(DateTime.Today, DateTime.Today + new TimeSpan(1, 0, 0, 0));
                var averages = await CommonMethods.GetAverageRocketLeagueStats(names, time, playedTogether: ConvertTogetherToBool(together));
                await ShowFavoriteStats(averages);
            }
            catch (Exception ex)
            {
                await SendMessageToCurrentChannelAsync(ex.Message);
            }
        }

        [Remarks(Constants.IgnoreEndpoint)]
        [Command("fav all")]
        [Summary("Gets your favorite stats for one or more players.")]
        public async Task StatsAllTime(string together, params string[] names)
        {
            try
            {
                var averages = await CommonMethods.GetAverageRocketLeagueStats(names, playedTogether: ConvertTogetherToBool(together));
                await ShowFavoriteStats(averages);
            }
            catch (Exception ex)
            {
                await SendMessageToCurrentChannelAsync(ex.Message);
            }
        }

        [Remarks(Constants.IgnoreEndpoint)]
        [Command("fav last")]
        [Summary("Gets your favorite stats for one or more players for the last [count] games.")]
        public async Task StatsLast(int count, string together, params string[] names)
        {
            try
            {
                var averages = await CommonMethods.GetAverageRocketLeagueStats(names, replayCap: count, playedTogether: ConvertTogetherToBool(together));
                await ShowFavoriteStats(averages);
            }
            catch (Exception ex)
            {
                await SendMessageToCurrentChannelAsync(ex.Message);
            }
        }

        [Command("set favorites")]
        [Alias("set fav")]
        [Summary("Sets and saves your favorite stats.")]
        public async Task SetFavoriteStats(string indexes)
        {
            try
            {
                var indexList = new List<string>(indexes.Split(',', StringSplitOptions.RemoveEmptyEntries));
                if (!indexList.Any())
                    return;
                var reader = new NumberListReader();

                var favorite = GetUserFavoriteByUserId(Context.User.Id);
                if (favorite is null)
                    favorite = new UserFavorite() { UserId = Context.User.Id };
                else
                    _userFavoritesConfigHandler.RemoveConfigEntry(favorite);

                favorite.FavoriteStats.Clear();
                favorite.AddPropertyNamesToConfigEntry(reader.ReadIndexNuberList(indexList), reader.CollectAll);

                _userFavoritesConfigHandler.AddConfigEntry(favorite);

                await SendMessageToCurrentChannelAsync("Successfully configured your favorite stats");
            }
            catch (Exception ex)
            {
                await SendMessageToCurrentChannelAsync(ex.Message);
            }
        }

        [Command("show favorites")]
        [Alias("show fav")]
        [Summary("shows your favorites stats")]
        public async Task ShowFavoriteStats()
        {
            try
            {
                var favorite = GetUserFavoriteByUserId(Context.User.Id);
                if (favorite is null)
                {
                    await SendMessageToCurrentChannelAsync(NoneConfiguredMessage);
                    return;
                }
                var builder = new StringBuilder($"{Context.User.Username}'s favorite stats:");
                foreach (var stat in favorite.FavoriteStats)
                    builder.Append('\n').Append(stat.Replace('_', ' '));
                await SendMessageToCurrentChannelAsync(builder.ToString());
            }
            catch (Exception ex)
            {
                await SendMessageToCurrentChannelAsync(ex.Message);
            }
        }

        private UserFavorite GetUserFavoriteByUserId(ulong userId)
        {
            foreach (var favorite in _userFavoritesConfigHandler.GetConfig())
            {
                if (favorite.UserId.Equals(userId))
                    return favorite;
            }
            return null;
        }

        [Command("show stats list")]
        [Alias("show sl")]
        [Summary("Shows a list of possible stats with their ids.")]
        public async Task ShowAllAvailableStatPropertiesAsync()
        {
            try
            {
                var tempFileName = RLStatsCommonMethods.GetAllAvailableStatPropertiesFilePath();
                await Context.Channel.SendFileAsync(new FileAttachment(tempFileName, "allProperties.json"), "All properties");
                File.Delete(tempFileName);
            }
            catch (Exception ex)
            {
                await SendMessageToCurrentChannelAsync(ex.Message);
            }
        }
    }
}
