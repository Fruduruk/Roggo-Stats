using System;
using System.Collections.Generic;

namespace Discord_Bot.Configuration
{
    public class UserFavorite : IEquatable<UserFavorite>
    {
        public ulong UserId { get; set; }
        public List<string> FavoriteStats { get; set; } = new List<string>();

        public override bool Equals(object obj)
        {
            return Equals(obj as UserFavorite);
        }

        public bool Equals(UserFavorite other)
        {
            if (other == null)
                return false;
            if (other.UserId != UserId)
                return false;
            if (FavoriteStats != other.FavoriteStats)
                return false;
            foreach (var item in FavoriteStats)
                if (!other.FavoriteStats.Contains(item))
                    return false;
            foreach (var item in other.FavoriteStats)
                if (!FavoriteStats.Contains(item))
                    return false;

            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(UserId, FavoriteStats);
        }
    }
}
