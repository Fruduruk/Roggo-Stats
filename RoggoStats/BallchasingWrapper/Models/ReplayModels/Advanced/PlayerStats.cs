namespace BallchasingWrapper.Models.ReplayModels.Advanced
{
    public class PlayerStats
    {
        [JsonProperty("core")] public PlayerCore PlayerCore { get; set; } = new();
        [JsonProperty("boost")] public PlayerBoost PlayerBoost { get; set; } = new();
        [JsonProperty("movement")] public PlayerMovement PlayerMovement { get; set; } = new();
        [JsonProperty("positioning")] public PlayerPositioning PlayerPositioning { get; set; } = new();
        [JsonProperty("demo")] public Demo Demo { get; set; } = new();
    }
}