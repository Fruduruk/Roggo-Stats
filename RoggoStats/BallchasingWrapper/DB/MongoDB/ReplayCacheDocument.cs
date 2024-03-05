using BallchasingWrapper.Models.ReplayModels;

namespace BallchasingWrapper.DB.MongoDB
{
    public class ReplayCacheDocument
    {
        public string Url { get; set; } = null!;
        public IEnumerable<Replay> Replays { get; set; } = null!;
    }
}
