namespace RLStats_Classes.Models.ReplayModels.Advanced
{
    public class Boost
    {
        public int? Bpm { get; set; }
        public float? Bcpm { get; set; }
        public float? Avg_amount { get; set; }
        public int? Amount_collected { get; set; }
        public int? Amount_stolen { get; set; }
        public int? Amount_collected_big { get; set; }
        public int? Amount_stolen_big { get; set; }
        public int? Amount_collected_small { get; set; }
        public int? Amount_stolen_small { get; set; }
        public int? Count_collected_big { get; set; }
        public int? Count_stolen_big { get; set; }
        public int? Count_collected_small { get; set; }
        public int? Count_stolen_small { get; set; }
        public int? Amount_overfill { get; set; }
        public int? Amount_overfill_stolen { get; set; }
        public int? Amount_used_while_supersonic { get; set; }
        public float? Time_zero_boost { get; set; }
        public float? Time_full_boost { get; set; }
        public float? Time_boost_0_25 { get; set; }
        public float? Time_boost_25_50 { get; set; }
        public float? Time_boost_50_75 { get; set; }
        public float? Time_boost_75_100 { get; set; }
    }
}
