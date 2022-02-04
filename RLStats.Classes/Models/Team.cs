using System;
using System.Collections.Generic;

namespace RLStats_Classes.Models
{
    public class Team
    {

        public int Goals { get; set; }
        public List<Player> Players { get; set; } = new List<Player>();
        public int InitialTeamSize { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            var that = obj as Team;
            if (Goals.Equals(that.Goals))
            {
                var playerList = GetBasicPlayersSorted();
                foreach (var p in playerList)
                {
                    if (!that.HasName(p.Name))
                        return false;
                }
                return true;
            }
            else
                return false;
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Goals);
            var playerList = GetBasicPlayersSorted();

            foreach (var p in playerList)
                hashCode.Add(p);
            var hash = hashCode.ToHashCode();
            return hash;
        }

        private IEnumerable<Player> GetBasicPlayersSorted()
        {
            var playerList = new List<Player>();
            for (int i = 0; i < Players.Count; i++)
            {
                if (playerList.Count < InitialTeamSize)
                    playerList.Add(Players[i]);
            }

            playerList.Sort();
            return playerList;
        }

        public bool HasName(string name)
        {
            foreach (var player in Players)
            {
                if (player.Name != null)
                    if (CheckEquality(player.Name,name))
                        return true;
            }
            return false;
        }

        private bool CheckEquality(object ob1, object ob2)
        {
            if (ob1 is null)
            {
                if (ob2 is null)
                    return true;
            }
            else
                if (ob1.Equals(ob2))
                return true;
            return false;
        }
    }
}
