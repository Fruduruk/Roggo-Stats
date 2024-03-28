namespace BallchasingWrapper.Models.ReplayModels.Advanced
{
    public class Ball
    {
        [JsonProperty("possession_time")] public float PossessionTime { get; set; }
        [JsonProperty("time_in_side")] public float TimeInSide { get; set; }
    }
}