using Discord;
using Discord.Commands;

using Discord_Bot.Configuration;
using Discord_Bot.ExtensionMethods;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Modules.RLStats
{
    [Remarks("HelperModule")]
    [Name("Subscription Commands - Private Only")]
    public class RecurringReportsModule : RlStatsModuleBase
    {
        private RecentlyAddedEntries _addedEntries;
        public RecurringReportsModule(ILogger<RecurringReportsModule> logger, string ballchasingToken, RecentlyAddedEntries entries) : base(logger, ballchasingToken)
        {
            _addedEntries = entries;
        }

        [Command("subscribe")]
        [Alias("sub")]
        [Summary("This command lets you subscribe a specific action")]
        public async Task Subscribe(string time, string together, params string[] names)
        {
            if (!Context.IsPrivate)
                return;
            if (!await CheckParameters(together, time, names))
                return;

            var config = ConfigHandler.Config;

            var configEntry = new ConfigEntry
            {
                Time = time,
                Together = ConvertTogetherToBool(together),
                Names = new List<string>(names),
                ChannelId = Context.Channel.Id,
                LastPost = DateTime.MinValue
            };

            if (config.HasConfigEntryInIt(configEntry))
            {
                await Context.Channel.SendMessageAsync("You have already subscribed to this.");
                return;
            }

            config.ConfigEntries.Add(configEntry);
            _addedEntries.ConfigEntries.Add(configEntry);
            ConfigHandler.Config = config;
            await Context.Channel.SendMessageAsync("Subscription saved.");
        }

        [Command("unsubscribe")]
        [Alias("unsub")]
        [Summary("This command lets you unsubscribe a specific action")]
        public async Task Unsubscribe(string time, string together, params string[] names)
        {
            if (!Context.IsPrivate)
                return;
            if (!await CheckParameters(together, time, names))
                return;
            var configEntry = new ConfigEntry
            {
                Time = time,
                Together = ConvertTogetherToBool(together),
                Names = new List<string>(names),
                ChannelId = Context.Channel.Id,
                LastPost = DateTime.MinValue
            };

            if (ConfigHandler.Config.HasConfigEntryInIt(configEntry))
            {
                ConfigHandler.RemoveConfigEntry(configEntry);
                await Context.Channel.SendMessageAsync("Subscription removed.");
                return;
            }
            await Context.Channel.SendMessageAsync("This subscription was not found.");
        }

        [Command("show subscriptions")]
        [Alias("show subs")]
        [Summary("Returns all subscriptions for this channel")]
        public async Task ShowSubscriptions()
        {
            var config = ConfigHandler.Config;
            foreach (var entry in config.ConfigEntries)
            {
                if (Context.Channel.Id != entry.ChannelId)
                    continue;
                var random = new Random(entry.GetHashCode());

                var builder = new EmbedBuilder()
                    .AddField("Names:", string.Join(',', entry.Names), inline: true)
                    .AddField("Together:", entry.Together, inline: true)
                    .AddField("Time:", entry.Time.Adverbify(), inline: true)
                    .AddField("Next Execution:", (entry.LastPost + entry.Time.ConvertTimeToTimeSpan()).ToString("dd.MM.yyyy HH:mm:ss"))
                    .WithColor(new Color(random.Next(255), random.Next(255), random.Next(255)));
                await Context.Channel.SendMessageAsync(embed: builder.Build());
            }
        }
    }
}
