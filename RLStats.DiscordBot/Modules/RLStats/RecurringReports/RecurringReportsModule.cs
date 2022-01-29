using Discord;
using Discord.Commands;

using Discord_Bot.Configuration;
using Discord_Bot.ExtensionMethods;
using Discord_Bot.RLStats;
using Discord_Bot.Singletons;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using RLStats_Classes.AverageModels;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            await ProceedToNextStepAsync(SubStepOneMessage, ExecuteSubStepOne, new ConfigEntry()
            {
                ChannelId = Context.Channel.Id,
                LastPost = DateTime.MinValue
            });
        }

        private async Task ProceedToNextStepAsync(string message, string stepCommand, ConfigEntry configEntry)
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

        [Remarks(ProceedingMethod)]
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

        [Remarks(ProceedingMethod)]
        [Command(ExecuteSubStepTwo)]
        public async Task ExecuteSubStepTwoAsync(string namesAndIds)
        {
            var stopHere = await IsProcessCanceledOrCorrupted(namesAndIds);
            if (stopHere)
                return;

            var configEntry = GetSavedConfigEntry();

            var nameAndIdArr = namesAndIds.Split(',');
            configEntry.Names = new List<string>(nameAndIdArr);

            await ProceedToNextStepAsync(SubStepThreeMessage(nameAndIdArr), ExecuteSubStepThree, configEntry);
        }

        [Remarks(ProceedingMethod)]
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


        [Remarks(ProceedingMethod)]
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

            await SendMessageToCurrentChannelAsync($"You have chosen these stats: {string.Join(',', configEntry.StatNames)}\n" +
                $"You have successfully configured your subscription.");

            if (ConfigHandler.Config.HasConfigEntryInIt(configEntry))
            {
                await Context.Channel.SendMessageAsync("You have already subscribed to this.");
                return;
            }

            ConfigHandler.AddConfigEntry(configEntry);
            _addedEntries.ConfigEntries.Add(configEntry);
        }

        private async Task ShowAllAvailableStatPropertiesAsync()
        {
            var dic = GetPropertyNameDictionary();
            var tempFileName = Path.Combine(RLStatsCommonMethods.GetRlStatsTempFolder(), "AllProperties.json");
            File.WriteAllText(tempFileName, JsonConvert.SerializeObject(dic, Formatting.Indented));

            await Context.Channel.SendFileAsync(new FileAttachment(tempFileName, "allProperties.json"), "All properties");
            File.Delete(tempFileName);
        }

        private static Dictionary<int, string> GetPropertyNameDictionary()
        {
            var properties = AveragePlayerStats.GetAllPropertyNames();
            var dic = new Dictionary<int, string>();
            for (int i = 0; i < properties.Length; i++)
                dic.Add(i, properties[i].Replace('_', ' '));
            return dic;
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
                .WithColor(new Color(random.Next(255), random.Next(255), random.Next(255)));
                await Context.Channel.SendMessageAsync(embed: builder.Build());
            }
        }
    }
}
