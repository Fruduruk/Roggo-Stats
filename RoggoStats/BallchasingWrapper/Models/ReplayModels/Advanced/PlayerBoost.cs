namespace BallchasingWrapper.Models.ReplayModels.Advanced
{
    public class PlayerBoost : Boost
    {
        [JsonProperty("percent_zero_boost")] public float PercentZeroBoost { get; set; }
        [JsonProperty("percent_full_boost")] public float PercentFullBoost { get; set; }
        [JsonProperty("percent_boost_0_25")] public float PercentBoost0To25 { get; set; }
        [JsonProperty("percent_boost_25_50")] public float PercentBoost25To50 { get; set; }
        [JsonProperty("percent_boost_50_75")] public float PercentBoost50To75 { get; set; }
        [JsonProperty("percent_boost_75_100")] public float PercentBoost75To100 { get; set; }
    }
}