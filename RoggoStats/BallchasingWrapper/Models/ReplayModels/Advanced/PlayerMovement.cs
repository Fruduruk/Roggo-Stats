namespace BallchasingWrapper.Models.ReplayModels.Advanced
{
    public class PlayerMovement : GeneralMovement
    {
        [JsonProperty("avg_speed")] public int AvgSpeed { get; set; }
        [JsonProperty("avg_powerslide_duration")] public float AvgPowerslideDuration { get; set; }
        [JsonProperty("avg_speed_percentage")] public float AvgSpeedPercentage { get; set; }
        [JsonProperty("percent_slow_speed")] public float PercentSlowSpeed { get; set; }
        [JsonProperty("percent_boost_speed")] public float PercentBoostSpeed { get; set; }
        [JsonProperty("percent_supersonic_speed")] public float PercentSupersonicSpeed { get; set; }
        [JsonProperty("percent_ground")] public float PercentGround { get; set; }
        [JsonProperty("percent_low_air")] public float PercentLowAir { get; set; }
        [JsonProperty("percent_high_air")] public float PercentHighAir { get; set; }
    }
}