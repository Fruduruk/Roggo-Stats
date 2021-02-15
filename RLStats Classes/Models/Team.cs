using System;
using System.Collections.Generic;

namespace RLStats_Classes.Models
{
    public class Team
    {

        public int Goals { get; set; }
        public List<Player> Players { get; set; } = new List<Player>();

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            var that = obj as Team;
            if (Goals.Equals(that.Goals))
            {
                foreach (var p in Players)
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
            return HashCode.Combine(Goals, Players);
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
