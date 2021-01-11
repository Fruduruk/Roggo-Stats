using System;
using System.Collections.Generic;
using System.Text;

namespace RocketLeagueStats.Models
{
    public class Team
    {

        public int Goals { get; set; }
        public List<Player> Players { get; set; } = new List<Player>();

        public bool HasName(string name)
        {
            foreach (var player in Players)
            {
                if (player.Name != null)
                    if (player.Name.ToLower().Equals(name.ToLower()))
                        return true;
            }
            return false;
        }
    }
}
