using BallchasingWrapper.Models.ReplayModels;

namespace BallchasingWrapper.Models
{
    public class ApiDataPack
    {
        [JsonProperty("list")]
        public List<Replay> Replays { get; set; } = new ();

        [JsonProperty("count")]
        public int ReplayCount { get; set; }

        [JsonProperty("next")]
        public string? Next { get; set; }
        public bool Success { get; set; }
        public string ReceivedString { get; set; } = string.Empty;
    }
}
