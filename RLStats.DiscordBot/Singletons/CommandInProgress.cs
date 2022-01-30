using Discord.Commands;

using Discord_Bot.Configuration;

namespace Discord_Bot.Singletons
{
    public class CommandInProgress
    {
        public ulong ChannelId { get; set; }
        public ulong UserId { get; set; }
        public string CommandToProceed { get; set; }
        public Subscription ConfigEntry { get; set; } = new Subscription();

        public CommandInProgress(ulong channelId, ulong userId, string commandToProceed)
        {
            ChannelId = channelId;
            UserId = userId;
            CommandToProceed = commandToProceed;
        }
    }
}
