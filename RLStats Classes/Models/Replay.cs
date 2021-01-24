using System;

namespace RLStats_Classes.Models
{
    public class Replay
    {
        public string ID { get; set; }
        public string Link { get; set; }
        public string Title { get; set; }
        public string Playlist { get; set; }
        public int Season { get; set; }
        public DateTime Date { get; set; }
        public string Uploader { get; set; }
        public Team Blue { get; set; }
        public Team Orange { get; set; }

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
