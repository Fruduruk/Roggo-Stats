using System.Collections.Generic;

namespace RLStats_Classes.Models
{
    public class CollectReplaysResponse
    {
        public IEnumerable<Replay> Replays { get; set; }
        public long ElapsedMilliseconds { get; set; }
        public int DoubleReplays { get; set; }
    }
}
