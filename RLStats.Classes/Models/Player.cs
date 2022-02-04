using System;

namespace RLStats_Classes.Models
{
    public class Player : IEquatable<Player>, IComparable<Player>
    {
        public double StartTime { get; set; }
        public double EndTime { get; set; }
        public string Name { get; set; }
        public bool MVP { get; set; }
        public int Score { get; set; }

        public int CompareTo(Player other)
        {
            if (other.Name is null && Name is null)
                return 0;
            if (other.Name is null && Name is not null)
                return -1;
            if (other.Name is not null && Name is null)
                return 1;
            return other.Name.CompareTo(Name);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Player);
        }

        public bool Equals(Player other)
        {
            if (other is null)
                return false;
            if (!Name.Equals(other.Name))
                return false;
            if (!MVP.Equals(other.MVP))
                return false;
            return true;
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Name);
            hashCode.Add(MVP);
            var hash = hashCode.ToHashCode();
            return hash;
        }
    }
}
