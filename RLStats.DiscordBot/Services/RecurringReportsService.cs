using Discord;
using Discord.Addons.Hosting;
using Discord.WebSocket;

using Discord_Bot.Configuration;
using Discord_Bot.Exceptions;
using Discord_Bot.ExtensionMethods;
using Discord_Bot.Singletons;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using RLStatsClasses.Interfaces;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Discord_Bot.Services
{
    public class RecurringReportsService : DiscordClientService
    {

        private RecentlyAddedEntries _addedEntries;
        private readonly RecurringReportServiceModule _module;
        private readonly ILogger<RecurringReportsService> _logger;
        private readonly ConfigHandler<Subscription> _configHandler;

        public RecurringReportsService(DiscordSocketClient client,
            ILogger<RecurringReportsService> logger,
            IDatabase database,
            IReplayCache replayCache,
            RecentlyAddedEntries recentlyAddedEntries,
            string ballchasingToken,
            ConfigHandler<Subscription> configHandler) : base(client, logger)
        {
            _addedEntries = recentlyAddedEntries;
            _module = new RecurringReportServiceModule(logger, database, replayCache, ballchasingToken);
            _logger = logger;
            _configHandler = configHandler;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting background threads in 5 seconds...");
            await Task.Delay(5000, stoppingToken);
            _logger.LogInformation("Starting background threads...");
            await StartBackgroundThreads(_configHandler.Config, stoppingToken);
            CheckForNewEntries(stoppingToken);
        }

        /// <summary>
        /// This Method executes the subscription commmands when they are ready.
        /// </summary>
        /// <param name="entry">The data the command is built of</param>
        /// <param name="channel">The channel in which the stats are sent to</param>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        private async Task ExecuteCommand(Subscription entry, IMessageChannel channel, CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Task {entry.Id} executes now...");

            await channel.TriggerTypingAsync();
            var filePaths = await _module.GetAverageStats(entry);
            if (filePaths.Any())
            {
                await channel.SendMessageAsync($"Hi, here is your {entry.Time.Adverbify()} report");

                try
                {
                    await SendFilesAsync(filePaths, channel);
                }
                catch (Exception ex)
                {
                    await channel.SendMessageAsync(ex.Message);
                }
            }
            else
            {
                //if (Debugger.IsAttached)
                await channel.SendMessageAsync($"Hi, here is your {entry.Time.Adverbify()} report. Well all averages are empty.");
            }
            _logger.LogInformation($"Task {entry.Id} executed.");
        }


        private async Task SendFilesAsync(IEnumerable<string> pathList, IMessageChannel channel)
        {
            foreach (var filePath in pathList)
            {
                await channel.SendFileAsync(filePath);
                if (File.Exists(filePath))
                    File.Delete(filePath);
                await Task.Delay(1337);
            }
        }

        private async void CheckForNewEntries(CancellationToken stoppingToken)
        {
            while (true)
            {
                try
                {
                    await Task.Delay(5000, stoppingToken);
                    if (stoppingToken.IsCancellationRequested)
                        break;
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                await StartBackgroundThreads(_addedEntries, stoppingToken);
                _addedEntries.Clear();
            }
        }

        private async Task StartBackgroundThreads(IEnumerable<Subscription> subscriptions, CancellationToken stoppingToken)
        {
            foreach (var entry in subscriptions)
            {
                var channel = await Client.GetChannelAsync(entry.ChannelId) as IMessageChannel;
                if (channel != null)
                {
                    _logger.LogInformation($"Starting background task for entry: {JsonConvert.SerializeObject(entry, Formatting.Indented)}");
                    StartBackgroundThread(entry, channel, stoppingToken);
                }
            }
        }

        private async void StartBackgroundThread(Subscription entry, IMessageChannel channel, CancellationToken stoppingToken)
        {
            while (true)
            {
                try
                {
                    var timeToWait = GetTimeToWait(entry.Time, entry.LastPost);
                    _logger.LogInformation($"Task {entry.Id} will wait {timeToWait.TotalDays} days.");
                    await Task.Delay(timeToWait, stoppingToken);
                    if (stoppingToken.IsCancellationRequested)
                        break;
                    if (!_configHandler.HasConfigEntryInIt(entry))
                        break;
                    entry = UpdateLastPost(entry, DateTime.Now);

                    lock (_module)
                    {
                        var executionTask = ExecuteCommand(entry, channel, stoppingToken);
                        executionTask.Wait();
                    }
                }
                catch (TaskCanceledException)
                {
                    _logger.LogInformation($"Task {entry.Id} was canceled");
                    break;
                }
                catch (EntryNotFoundException ex)
                {
                    _logger.LogError(ex, $"Task {entry.Id} was canceled due to an error.");
                    break;
                }
            }
        }

        public Subscription UpdateLastPost(Subscription entry, DateTime newLastPost)
        {
            Subscription newEntry = null;
            var config = _configHandler.Config;
            foreach (var configEntry in config)
            {
                if (configEntry.Equals(entry))
                {
                    configEntry.LastPost = newLastPost;
                    newEntry = configEntry;
                }
            }
            _configHandler.SaveConfigFile(config);
            if (newEntry is null)
                throw new EntryNotFoundException();
            return newEntry;
        }

        private static TimeSpan GetTimeToWait(string time, DateTime lastPost)
        {
            var now = DateTime.Now;
            var timeToWait = time.ConvertTimeToTimeSpan();
            if (lastPost + timeToWait < now)
                return TimeSpan.Zero;
            else
                return (lastPost + timeToWait) - now;
        }
    }
}
