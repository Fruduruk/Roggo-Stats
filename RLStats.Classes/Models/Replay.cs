using System;

namespace RLStats_Classes.Models
{
    public class Replay
    {
        public string Id { get; set; }
        public string RocketLeagueId { get; set; }
        public string SeasonType { get; set; }
        public string Visibility { get; set; }
        public string Link { get; set; }
        public string Title { get; set; }
        public string Playlist { get; set; }
        public int Season { get; set; }
        public DateTime Date { get; set; }
        public string Uploader { get; set; }
        public Team Blue { get; set; }
        public Team Orange { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            var that = obj as Replay;
            if (that.Id.Equals(Id))
                return true;
            if (CheckEquality(SeasonType, that.SeasonType))
                if (Season.Equals(that.Season))
                    if (CheckEquality(Playlist, that.Playlist))
                        if (CheckEquality(Blue, that.Blue))
                            if (CheckEquality(Orange, that.Orange))
                                return true;
            return false;
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(SeasonType);
            hashCode.Add(Playlist);
            hashCode.Add(Season);
            hashCode.Add(Blue);
            hashCode.Add(Orange);
            var hash = hashCode.ToHashCode();
            return hash;
        }

        private static bool CheckEquality(object ob1, object ob2)
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

        public bool HasNameInIt(string name)
        {
            if (Blue.HasName(name))
                return true;
            if (Orange.HasName(name))
                return true;
            return false;
        }
    }
}
