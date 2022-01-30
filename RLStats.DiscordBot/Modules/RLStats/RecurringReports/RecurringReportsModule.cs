using Discord;
using Discord.Commands;

using Discord_Bot.Configuration;
using Discord_Bot.ExtensionMethods;
using Discord_Bot.RLStats;
using Discord_Bot.Singletons;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private readonly ConfigHandler<Subscription> _configHandler;

        public RecurringReportsModule(ILogger<RecurringReportsModule> logger, string ballchasingToken, RecentlyAddedEntries entries, CommandsToProceed commandsToProceed, ConfigHandler<Subscription> configHandler) : base(logger, ballchasingToken)
        {
            _addedEntries = entries;
            _commandsToProceed = commandsToProceed;
            _configHandler = configHandler;
        }

        [Command("subscribe")]
        [Alias("sub")]
        [Summary("This command lets you subscribe a specific action")]
        public async Task Subscribe()
        {
            if (!Context.IsPrivate)
                return;

            await ProceedToNextStepAsync(SubStepOneMessage, ExecuteSubStepOne, new Subscription()
            {
                Id = GetUnusedId(),
                ChannelId = Context.Channel.Id,
                LastPost = DateTime.MinValue
            });
        }

        private int GetUnusedId()
        {
            int id = 1;
            foreach (var entry in _configHandler.Config)
            {
                if (!entry.Id.Equals(id))
                    return id;
                id++;
            }
            return id;
        }

        private async Task ProceedToNextStepAsync(string message, string stepCommand, Subscription configEntry)
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

            var commandInProgress = new CommandInProgress(Context.Channel.Id, Context.User.Id, stepCommand)
            {
                ConfigEntry = configEntry
            };
            _commandsToProceed.CommandsInProgress.Add(commandInProgress);
        }

        private Subscription GetSavedConfigEntry()
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

        private void RemoveSavedConfigEntry()
        {
            CommandInProgress commandToRemove = null;
            foreach (var command in _commandsToProceed.CommandsInProgress)
            {
                if (command.UserId == Context.User.Id && command.ChannelId == Context.Channel.Id)
                {
                    commandToRemove = command;
                }
            }
            if (commandToRemove != null)
                _commandsToProceed.CommandsInProgress.Remove(commandToRemove);
        }

        [Remarks(Constants.IgnoreEndpoint)]
        [Command(ExecuteSubStepOne)]
        public async Task ExecuteSubStepOneAsync(string time)
        {
            var stopHere = await IsProcessCanceledOrCorrupted(time);
            if (stopHere)
                return;

            var configEntry = GetSavedConfigEntry();

            if (!time.CanConvertTime())
            {
                await SendMessageToCurrentChannelAsync($"{time} is not a valid time value. Try d,w,m or y");
                return;
            }

            configEntry.Time = time;

            await ProceedToNextStepAsync(SubStepTwoMessage(time), ExecuteSubStepTwo, configEntry);
        }

        [Remarks(Constants.IgnoreEndpoint)]
        [Command(ExecuteSubStepTwo)]
        public async Task ExecuteSubStepTwoAsync(string namesAndIds)
        {
            var stopHere = await IsProcessCanceledOrCorrupted(namesAndIds);
            if (stopHere)
                return;

            var configEntry = GetSavedConfigEntry();

            var nameAndIdArr = namesAndIds.Split(',');
            if (!nameAndIdArr.Any())
                return;

            configEntry.Names = new List<string>(nameAndIdArr);

            if (configEntry.Names.Count > 1)
                await ProceedToNextStepAsync(SubStepThreeMessage(nameAndIdArr), ExecuteSubStepThree, configEntry);
            else
            {
                await ProceedToNextStepAsync(SubStepSkipThreeMessage(nameAndIdArr), ExecuteSubStepFour, configEntry);
                await ShowAllAvailableStatPropertiesAsync();
            }
        }

        [Remarks(Constants.IgnoreEndpoint)]
        [Command(ExecuteSubStepThree)]
        public async Task ExecuteSubStepThreeAsync(string together)
        {
            var stopHere = await IsProcessCanceledOrCorrupted(together);
            if (stopHere)
                return;

            var configEntry = GetSavedConfigEntry();


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

            await ProceedToNextStepAsync(SubStepFourMessage(playedTogether), ExecuteSubStepFour, configEntry);

            await ShowAllAvailableStatPropertiesAsync();
        }

        private async Task ShowAllAvailableStatPropertiesAsync()
        {
            var tempFileName = RLStatsCommonMethods.GetAllAvailableStatPropertiesFilePath();
            await Context.Channel.SendFileAsync(new FileAttachment(tempFileName, "allProperties.json"), "All properties");
            File.Delete(tempFileName);
        }

        [Remarks(Constants.IgnoreEndpoint)]
        [Command(ExecuteSubStepFour)]
        public async Task ExecuteSubStepFourAsync(string indexes)
        {
            var stopHere = await IsProcessCanceledOrCorrupted(indexes);
            if (stopHere)
                return;

            var configEntry = GetSavedConfigEntry();
            var indexList = new List<string>(indexes.Split(',', StringSplitOptions.RemoveEmptyEntries));
            if (!indexList.Any())
                return;
            var reader = new NumberListReader();
            configEntry.AddPropertyNamesToConfigEntry(reader.ReadIndexNuberList(indexList), reader.CollectAll);

            await SendMessageToCurrentChannelAsync("You have successfully configured your subscription.");

            if (_configHandler.HasConfigEntryInIt(configEntry))
            {
                await Context.Channel.SendMessageAsync("You have already subscribed to this.");
                return;
            }

            _configHandler.AddConfigEntry(configEntry);
            _addedEntries.Add(configEntry);
            RemoveSavedConfigEntry();
        }

        private async Task<bool> IsProcessCanceledOrCorrupted(string input)
        {
            var configEntry = GetSavedConfigEntry();
            if (configEntry is null)
            {
                await SendMessageToCurrentChannelAsync(CorruptedConfigMessage);
                return true;
            }

            if (string.IsNullOrEmpty(input))
                return true;

            if ("cancel".Equals(input))
            {
                await SendMessageToCurrentChannelAsync(ProcessCanceledMessage);
                RemoveSavedConfigEntry();
                return true;
            }

            return false;
        }

        [Command("unsubscribe all")]
        [Alias("unsub all")]
        [Summary("This command lets you unsubscribe all subscriptions")]
        public async Task UnsubscribeAll()
        {
            if (!Context.IsPrivate)
                return;
            var entriesNotFromThisChannel = new List<Subscription>();
            foreach (var entry in _configHandler.Config)
            {
                if (Context.Channel.Id.Equals(entry.ChannelId))
                    continue;
                entriesNotFromThisChannel.Add(entry);
            }
            _configHandler.SaveConfigFile(entriesNotFromThisChannel);

            await Context.Channel.SendMessageAsync("Removed all subscriptions from this channel.");
        }

        [Command("unsubscribe")]
        [Alias("unsub")]
        [Summary("This command lets you unsubscribe a subscription")]
        public async Task Unsubscribe(int id)
        {
            Subscription entryToRemove = null;
            foreach (var entry in _configHandler.Config)
            {
                if (entry.ChannelId.Equals(Context.Channel.Id) && entry.Id.Equals(id))
                    entryToRemove = entry;
            }
            if (entryToRemove is null)
            {
                await SendMessageToCurrentChannelAsync($"There is no subscription with id {id}");
                return;
            }
            _configHandler.RemoveConfigEntry(entryToRemove);
            await SendMessageToCurrentChannelAsync($"Successfully removed subscription with id {id}");
        }


        [Command("show subscriptions")]
        [Alias("show subs")]
        [Summary("Returns all subscriptions for this channel")]
        public async Task ShowSubscriptions()
        {
            foreach (var entry in _configHandler.Config)
            {
                if (Context.Channel.Id != entry.ChannelId)
                    continue;
                var random = new Random(entry.GetHashCode());

                var builder = new EmbedBuilder()
                    .AddField("Time:", entry.Time.Adverbify(), inline: true)
                    .AddField("Names:", string.Join(',', entry.Names), inline: true)
                    .AddField("Together:", entry.Together, inline: true);

                if (entry.StatNames.Count < 21)
                {
                    var fancyStatNames = new List<string>();
                    foreach (var statName in entry.StatNames)
                        fancyStatNames.Add(statName.Replace('_', ' '));
                    builder.AddField("Stats:", string.Join('\n', fancyStatNames), inline: true);
                }
                else
                    builder.AddField("Stats count:", entry.StatNames.Count, inline: true);

                builder.AddField("Next Execution:", (entry.LastPost + entry.Time.ConvertTimeToTimeSpan()).ToString("dd.MM.yyyy HH:mm:ss"))
                .WithColor(new Color(random.Next(255), random.Next(255), random.Next(255)))
                .WithFooter($"Id: {entry.Id}");
                await Context.Channel.SendMessageAsync(embed: builder.Build());
            }
        }
    }
}
