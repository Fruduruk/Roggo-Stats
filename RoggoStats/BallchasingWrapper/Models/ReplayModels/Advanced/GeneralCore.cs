namespace BallchasingWrapper.Models.ReplayModels.Advanced
{
    public class GeneralCore
    {
        [JsonProperty("shots")] public int Shots { get; set; }
        [JsonProperty("shots_against")] public int ShotsAgainst { get; set; }
        [JsonProperty("goals")] public int Goals { get; set; }
        [JsonProperty("goals_against")] public int GoalsAgainst { get; set; }
        [JsonProperty("saves")] public int Saves { get; set; }
        [JsonProperty("assists")] public int Assists { get; set; }
        [JsonProperty("score")] public int Score { get; set; }
        [JsonProperty("shooting_percentage")] public float ShootingPercentage { get; set; }
    }
}