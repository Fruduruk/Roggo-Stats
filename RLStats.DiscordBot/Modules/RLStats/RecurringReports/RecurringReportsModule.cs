using Discord;
using Discord.Commands;

using Discord_Bot.Configuration;
using Discord_Bot.ExtensionMethods;
using Discord_Bot.Singletons;

using Microsoft.Extensions.Logging;

using RLStats_Classes.AverageModels;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using static Discord_Bot.Modules.RLStats.RecurringReports.RecurringReportsConstants;

namespace Discord_Bot.Modules.RLStats.RecurringReports
{
    [Remarks("HelperModule")]
    [Name("Subscription Commands - Private Only")]
    public class RecurringReportsModule : RlStatsModuleBase
    {
        private RecentlyAddedEntries _addedEntries;
        private readonly CommandsToProceed _commandsToProceed;

        public RecurringReportsModule(ILogger<RecurringReportsModule> logger, string ballchasingToken, RecentlyAddedEntries entries, CommandsToProceed commandsToProceed) : base(logger, ballchasingToken)
        {
            _addedEntries = entries;
            _commandsToProceed = commandsToProceed;
        }

        [Command("subscribe")]
        [Alias("sub")]
        [Summary("This command lets you subscribe a specific action")]
        public async Task Subscribe()
        {
            if (!Context.IsPrivate)
                return;
            await ProceedToNextStep(SubStepOneMessage, ExecuteSubStepOne, new ConfigEntry()
            {
                ChannelId = Context.Channel.Id
            });
        }

        private async Task ProceedToNextStep(string message, string stepCommand, ConfigEntry configEntry)
        {
            await SendMessageToCurrentChannelAsync(message);
            foreach (var command in _commandsToProceed.CommandsInProgress)
            {
                if (command.UserId == Context.User.Id && command.ChannelId == Context.Channel.Id)
                {
                    command.CommandToProceed = stepCommand;
                    command.ConfigEntry = configEntry;
                    return;
                }
            }

            CommandInProgress commandInProgress = new CommandInProgress(Context.Channel.Id, Context.User.Id, stepCommand)
            {
                ConfigEntry = configEntry
            };
            _commandsToProceed.CommandsInProgress.Add(commandInProgress);
        }

        private ConfigEntry GetSavedConfigEntry()
        {
            foreach (var command in _commandsToProceed.CommandsInProgress)
            {
                if (command.UserId == Context.User.Id && command.ChannelId == Context.Channel.Id)
                {
                    return command.ConfigEntry;
                }
            }

            return null;
        }

        [Command(ExecuteSubStepOne)]
        public async Task ExecuteSubStepOneAsync(string time)
        {
            var configEntry = GetSavedConfigEntry();
            if (configEntry is null)
            {
                await SendMessageToCurrentChannelAsync(CorruptedConfigMessage);
                return;
            }

            if (string.IsNullOrEmpty(time))
                return;

            if (!time.CanConvertTime())
            {
                await SendMessageToCurrentChannelAsync($"{time} is not a valid time value. Try d,w,m or y");
                return;
            }

            configEntry.Time = time;

            await ProceedToNextStep(SubStepTwoMessage(time), ExecuteSubStepTwo, configEntry);
        }

        [Command(ExecuteSubStepTwo)]
        public async Task ExecuteSubStepTwoAsync(string namesAndIds)
        {
            var configEntry = GetSavedConfigEntry();
            if (configEntry is null)
            {
                await SendMessageToCurrentChannelAsync(CorruptedConfigMessage);
                return;
            }

            if (string.IsNullOrEmpty(namesAndIds))
                return;

            var nameAndIdArr = namesAndIds.Split(',');
            configEntry.Names = new List<string>(nameAndIdArr);

            await ProceedToNextStep(SubStepThreeMessage(nameAndIdArr), ExecuteSubStepThree, configEntry);
        }

        [Command(ExecuteSubStepThree)]
        public async Task ExecuteSubStepThreeAsync(string together)
        {
            var configEntry = GetSavedConfigEntry();
            if (configEntry is null)
            {
                await SendMessageToCurrentChannelAsync(CorruptedConfigMessage);
                return;
            }

            if (string.IsNullOrEmpty(together))
                return;

            bool playedTogether;
            try
            {
                playedTogether = ConvertTogetherToBool(together);

            }
            catch (ArgumentOutOfRangeException ex)
            {
                await SendMessageToCurrentChannelAsync(ex.Message);
                return;
            }

            configEntry.Together = playedTogether;
            
            await ProceedToNextStep(SubStepFourMessage(playedTogether), ExecuteSubStepFour, configEntry);
        }

        public async Task ExecuteSubStepFourAsync(string indexes)
        {
            
        }

        //var config = ConfigHandler.Config;

        //var configEntry = new ConfigEntry
        //{
        //    Time = time,
        //    Together = ConvertTogetherToBool(together),
        //    Names = new List<string>(names),
        //    ChannelId = Context.Channel.Id,
        //    LastPost = DateTime.MinValue
        //};

        //if (config.HasConfigEntryInIt(configEntry))
        //{
        //    await Context.Channel.SendMessageAsync("You have already subscribed to this.");
        //    return;
        //}

        //config.ConfigEntries.Add(configEntry);
        //_addedEntries.ConfigEntries.Add(configEntry);
        //ConfigHandler.Config = config;
        //await Context.Channel.SendMessageAsync("Subscription saved.");








        //[Command("unsubscribe")]
        //[Alias("unsub")]
        //[Summary("This command lets you unsubscribe a specific action")]
        //public async Task Unsubscribe(string time, string together, params string[] names)
        //{
        //    if (!Context.IsPrivate)
        //        return;
        //    if (!await CheckParameters(together, time, names))
        //        return;
        //    var configEntry = new ConfigEntry
        //    {
        //        Time = time,
        //        Together = ConvertTogetherToBool(together),
        //        Names = new List<string>(names),
        //        ChannelId = Context.Channel.Id,
        //        LastPost = DateTime.MinValue
        //    };

        //    if (ConfigHandler.Config.HasConfigEntryInIt(configEntry))
        //    {
        //        ConfigHandler.RemoveConfigEntry(configEntry);
        //        await Context.Channel.SendMessageAsync("Subscription removed.");
        //        return;
        //    }
        //    await Context.Channel.SendMessageAsync("This subscription was not found.");
        //}

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
