namespace RocketLeagueStats.AdvancedModels
{
    public class PlayerMovement : GeneralMovement
    {
        public int? Avg_speed { get; set; }
        public float? Avg_powerslide_duration { get; set; }
        public float? Avg_speed_percentage { get; set; }
        public float? Percent_slow_speed { get; set; }
        public float? Percent_boost_speed { get; set; }
        public float? Percent_supersonic_speed { get; set; }
        public float? Percent_ground { get; set; }
        public float? Percent_low_air { get; set; }
        public float? Percent_high_air { get; set; }
    }
}