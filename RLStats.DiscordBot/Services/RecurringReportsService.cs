using Discord;
using Discord.Addons.Hosting;
using Discord.WebSocket;

using Discord_Bot.Configuration;
using Discord_Bot.Exceptions;
using Discord_Bot.ExtensionMethods;

using Microsoft.Extensions.Logging;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Discord_Bot.Services
{
    public class RecurringReportsService : DiscordClientService
    {
        private readonly object Lock = new object();
        private RecentlyAddedEntries _addedEntries;
        public RecurringReportsService(DiscordSocketClient client, ILogger<DiscordClientService> logger, RecentlyAddedEntries recentlyAddedEntries) : base(client, logger)
        {
            _addedEntries = recentlyAddedEntries;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await StartBackgroundThreads(ConfigHandler.Config, stoppingToken);
            CheckForNewEntries(stoppingToken);
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
                _addedEntries.ConfigEntries.Clear();
            }
        }

        private async Task StartBackgroundThreads(Config config, CancellationToken stoppingToken)
        {
            foreach (var entry in config.ConfigEntries)
            {
                var channel = await Client.GetChannelAsync(entry.ChannelId) as IMessageChannel;
                if (channel != null)
                {
                    StartBackgroundThread(entry, channel, stoppingToken);
                }
            }
        }

        private async void StartBackgroundThread(ConfigEntry entry, IMessageChannel channel, CancellationToken stoppingToken)
        {
            while (true)
            {
                try
                {
                    await Task.Delay(GetTimeToWait(entry.Time, entry.LastPost), stoppingToken);
                    if (stoppingToken.IsCancellationRequested)
                        break;
                    if (!ConfigHandler.Config.HasConfigEntryInIt(entry))
                        break;
                    entry = await Task.Run(() => ConfigHandler.UpdateLastPost(entry, DateTime.Now));

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

        private async Task ExecuteCommand(ConfigEntry entry, IMessageChannel channel, CancellationToken stoppingToken)
        {
            //lock (Lock) ONLY ONE AT A TIME
            {
                await channel.SendMessageAsync($"Hi, here is your {entry.Time.Adverbify()} report");

            }
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
