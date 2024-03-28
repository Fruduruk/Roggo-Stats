namespace BallchasingWrapper.Models.ReplayModels.Advanced
{
    public class AdvancedTeam
    {
        [JsonProperty("color")] public string Color { get; set; } = string.Empty;
        [JsonProperty("players")] public List<AdvancedPlayer> Players { get; set; } = new();
        [JsonProperty("stats")] public TeamStats Stats { get; set; } = new();
    }
}