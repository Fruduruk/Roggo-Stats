namespace BallchasingWrapper.Models.ReplayModels.Advanced
{
    public class GeneralBoost
    {
        [JsonProperty("bpm")] public int Bpm { get; set; }
        [JsonProperty("bcpm")] public float Bcpm { get; set; }
        [JsonProperty("avg_amount")] public float AvgAmount { get; set; }
        [JsonProperty("amount_collected")] public int AmountCollected { get; set; }
        [JsonProperty("amount_stolen")] public int AmountStolen { get; set; }
        [JsonProperty("amount_collected_big")] public int AmountCollectedBig { get; set; }
        [JsonProperty("amount_stolen_big")] public int AmountStolenBig { get; set; }
        [JsonProperty("amount_collected_small")] public int AmountCollectedSmall { get; set; }
        [JsonProperty("amount_stolen_small")] public int AmountStolenSmall { get; set; }
        [JsonProperty("count_collected_big")] public int CountCollectedBig { get; set; }
        [JsonProperty("count_stolen_big")] public int CountStolenBig { get; set; }
        [JsonProperty("count_collected_small")] public int CountCollectedSmall { get; set; }
        [JsonProperty("count_stolen_small")] public int CountStolenSmall { get; set; }
        [JsonProperty("amount_overfill")] public int AmountOverfill { get; set; }
        [JsonProperty("amount_overfill_stolen")] public int AmountOverfillStolen { get; set; }
        [JsonProperty("amount_used_while_supersonic")] public int AmountUsedWhileSupersonic { get; set; }
        [JsonProperty("time_zero_boost")] public float TimeZeroBoost { get; set; }
        [JsonProperty("time_full_boost")] public float TimeFullBoost { get; set; }
        [JsonProperty("time_boost_0_25")] public float TimeBoost0To25 { get; set; }
        [JsonProperty("time_boost_25_50")] public float TimeBoost25To50 { get; set; }
        [JsonProperty("time_boost_50_75")] public float TimeBoost50To75 { get; set; }
        [JsonProperty("time_boost_75_100")] public float TimeBoost75To100 { get; set; }
    }
}