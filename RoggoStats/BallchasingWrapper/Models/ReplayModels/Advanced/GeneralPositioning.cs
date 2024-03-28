namespace BallchasingWrapper.Models.ReplayModels.Advanced
{
    public class GeneralPositioning
    {
        [JsonProperty("time_defensive_third")] public float TimeDefensiveThird { get; set; }
        [JsonProperty("time_neutral_third")] public float TimeNeutralThird { get; set; }
        [JsonProperty("time_offensive_third")] public float TimeOffensiveThird { get; set; }
        [JsonProperty("time_defensive_half")] public float TimeDefensiveHalf { get; set; }
        [JsonProperty("time_offensive_half")] public float TimeOffensiveHalf { get; set; }
        [JsonProperty("time_behind_ball")] public float TimeBehindBall { get; set; }
        [JsonProperty("time_infront_ball")] public float TimeInfrontBall { get; set; }
    }
}