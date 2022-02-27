using RLStatsClasses.Models.ReplayModels;

using System.Collections.Generic;

namespace RLStatsClasses.Models
{
    public class CollectReplaysResponse
    {
        public IEnumerable<Replay> Replays { get; set; } = new List<Replay>();
        public long ElapsedMilliseconds { get; set; }
        public int DoubleReplays { get; set; }
    }
}
