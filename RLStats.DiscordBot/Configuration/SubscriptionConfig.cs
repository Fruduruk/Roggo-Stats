using System.Collections.Generic;

namespace Discord_Bot.Configuration
{
    public class SubscriptionConfig : IConfigList<SubscriptionConfigEntry>
    {
        public List<SubscriptionConfigEntry> ConfigEntries { get; set; } = new List<SubscriptionConfigEntry>();
    }
}
