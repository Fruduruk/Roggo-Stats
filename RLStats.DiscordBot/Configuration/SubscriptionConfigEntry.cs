using System;
using System.Collections.Generic;

namespace Discord_Bot.Configuration
{
    public class SubscriptionConfigEntry : IEquatable<SubscriptionConfigEntry>
    {
        public int Id { get; set; }
        public string Time { get; set; }
        public bool Together { get; set; }
        public List<string> Names { get; set; } = new List<string>();
        public ulong ChannelId { get; set; }
        public List<string> StatNames { get; set; } = new List<string>();
        public DateTime LastPost { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as SubscriptionConfigEntry);
        }

        public bool Equals(SubscriptionConfigEntry other)
        {
            if (other is null)
                return false;
            if (other.Time != Time)
                return false;
            if (other.Together != Together)
                return false;
            if (other.ChannelId != ChannelId)
                return false;
            foreach (string name in Names)
                if (!other.Names.Contains(name))
                    return false;
            foreach (string name in other.Names)
                if (!Names.Contains(name))
                    return false;

            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Time, Together, Names, ChannelId, LastPost, StatNames);
        }
    }
}
