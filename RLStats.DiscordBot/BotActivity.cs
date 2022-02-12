using Discord;

namespace Discord_Bot
{
    public class BotActivity : IActivity
    {
        public string Name => " Rocket League Replays";

        public ActivityType Type => ActivityType.Watching;

        public ActivityProperties Flags => ActivityProperties.None;

        public string Details => "This bot is crushing numbers from Rocket League replays.";
    }
}
