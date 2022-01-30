using Discord;
using Discord.Addons.Hosting;
using Discord.WebSocket;

using Discord_Bot.Configuration;
using Discord_Bot.Exceptions;
using Discord_Bot.ExtensionMethods;
using Discord_Bot.Singletons;

using Microsoft.Extensions.Logging;

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
        private readonly ConfigHandler<Subscription> _configHandler;

        public RecurringReportsService(DiscordSocketClient client, ILogger<DiscordClientService> logger, RecentlyAddedEntries recentlyAddedEntries, string ballchasingToken, ConfigHandler<Subscription> configHandler) : base(client, logger)
        {
            _addedEntries = recentlyAddedEntries;
            _module = new RecurringReportServiceModule(logger, ballchasingToken);
            _configHandler = configHandler;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(5000, stoppingToken);
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
                    await Task.Delay(GetTimeToWait(entry.Time, entry.LastPost), stoppingToken);
                    if (stoppingToken.IsCancellationRequested)
                        break;
                    if (!_configHandler.HasConfigEntryInIt(entry))
                        break;
                    entry = UpdateLastPost(entry, DateTime.Now);

                    await ExecuteCommand(entry, channel, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (EntryNotFoundException)
                {
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
