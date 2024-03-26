using BallchasingWrapper.Models.ReplayModels;

namespace BallchasingWrapper.DB.MongoDB
{
    public class ReplayCacheDocument
    {
        public string? FilterString { get; set; } = null!;
        public IEnumerable<Replay>? Replays { get; set; }
    }
}
