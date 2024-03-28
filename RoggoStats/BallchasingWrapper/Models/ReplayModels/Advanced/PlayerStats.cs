namespace BallchasingWrapper.Models.ReplayModels.Advanced
{
    public class PlayerStats
    {
        [JsonProperty("core")] public PlayerCore Core { get; set; } = new();
        [JsonProperty("boost")] public PlayerBoost Boost { get; set; } = new();
        [JsonProperty("movement")] public PlayerMovement Movement { get; set; } = new();
        [JsonProperty("positioning")] public PlayerPositioning Positioning { get; set; } = new();
        [JsonProperty("demo")] public Demo Demo { get; set; } = new();
    }
}