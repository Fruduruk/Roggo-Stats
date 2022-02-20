
using System;

namespace RLStatsClasses.CacheHandlers
{
    public class CacheEntry : IEquatable<CacheEntry>
    {
        public string URL { get; set; }
        public string FileName { get; set; }

        public bool Equals(CacheEntry other)
        {
            if (other is null)
                return false;
            if (other.URL == URL)
                if (other.FileName == FileName)
                    return true;
            return false;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CacheEntry);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(URL);
            hashCode.Add(FileName);
            return hashCode.ToHashCode();
        }
    }
}
