namespace BallchasingWrapper.Models.ReplayModels.Advanced
{
    public class GeneralMovement
    {
        [JsonProperty("total_distance")] public int TotalDistance { get; set; }
        [JsonProperty("time_supersonic_speed")] public float TimeSupersonicSpeed { get; set; }
        [JsonProperty("time_boost_speed")] public float TimeBoostSpeed { get; set; }
        [JsonProperty("time_slow_speed")] public float TimeSlowSpeed { get; set; }
        [JsonProperty("time_ground")] public float TimeGround { get; set; }
        [JsonProperty("time_low_air")] public float TimeLowAir { get; set; }
        [JsonProperty("time_high_air")] public float TimeHighAir { get; set; }
        [JsonProperty("time_powerslide")] public float TimePowerslide { get; set; }
        [JsonProperty("count_powerslide")] public int CountPowerslide { get; set; }
    }
}