using RLStats_Classes.Models.ReplayModels;

using System.Collections.Generic;

namespace RLStats_Classes.Models
{
    public class CollectReplaysResponse
    {
        public IEnumerable<Replay> Replays { get; set; } = new List<Replay>();
        public long ElapsedMilliseconds { get; set; }
        public int DoubleReplays { get; set; }
    }
}
