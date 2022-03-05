using System;
using System.Collections.Generic;

namespace Discord_Bot.Configuration
{
    public class Subscription : IEquatable<Subscription>
    {
        public int Id { get; set; }
        public string Time { get; set; }
        public bool Together { get; set; }
        public List<string> Names { get; set; } = new List<string>();
        public ulong ChannelId { get; set; }
        public List<string> StatNames { get; set; } = new List<string>();
        public DateTime LastPost { get; set; }
        public bool CompareToLastTime { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as Subscription);
        }

        public bool Equals(Subscription other)
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
            if (other.CompareToLastTime != CompareToLastTime)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Time, Together, Names, ChannelId, LastPost, StatNames, CompareToLastTime);
        }
    }
}
