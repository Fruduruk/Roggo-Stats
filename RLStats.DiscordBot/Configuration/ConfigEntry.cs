using System;
using System.Collections.Generic;

namespace Discord_Bot.Configuration
{
    public class ConfigEntry
    {
        public string Time { get; set; }
        public bool Together { get; set; }
        public List<string> Names { get; set; } = new List<string>();
        public ulong ChannelId { get; set; }
        public List<string> StatNames { get; set; } = new List<string>();
        public DateTime LastPost { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is ConfigEntry ce)
            {
                if (ce.Time != Time)
                    return false;
                if (ce.Together != Together)
                    return false;
                if (ce.ChannelId != ChannelId)
                    return false;
                foreach (string name in Names)
                    if (!ce.Names.Contains(name))
                        return false;
                foreach (string name in ce.Names)
                    if (!Names.Contains(name))
                        return false;
            }
            else
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Time, Together, Names, ChannelId, LastPost, StatNames);
        }
    }
}
