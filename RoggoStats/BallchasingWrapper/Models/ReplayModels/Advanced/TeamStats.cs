namespace BallchasingWrapper.Models.ReplayModels.Advanced
{
    public class TeamStats
    {
        [JsonProperty("ball")] public Ball Ball { get; set; } = new();
        [JsonProperty("core")] public Core Core { get; set; } = new();
        [JsonProperty("boost")] public Boost Boost { get; set; } = new();
        [JsonProperty("movement")] public GeneralMovement Movement { get; set; } = new();
        [JsonProperty("positioning")] public GeneralPositioning Positioning { get; set; } = new();
        [JsonProperty("demo")] public Demo Demo { get; set; } = new();
    }
}