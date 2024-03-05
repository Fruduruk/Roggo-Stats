using BallchasingWrapper.Models.ReplayModels;

namespace BallchasingWrapper.Models
{
    public class CollectReplaysResponse
    {
        public IEnumerable<Replay> Replays { get; set; } = new List<Replay>();
        public long ElapsedMilliseconds { get; set; }
        public int DoubleReplays { get; set; }
    }
}
