namespace BallchasingWrapper.Models.ReplayModels.Advanced
{
    public class PlayerPositioning : GeneralPositioning
    {
        public int? Avg_distance_to_ball { get; set; }
        public int? Avg_distance_to_ball_possession { get; set; }
        public int? Avg_distance_to_ball_no_possession { get; set; }
        public int? Avg_distance_to_mates { get; set; }
        public float? Time_most_back { get; set; }
        public float? Time_most_forward { get; set; }
        public float? Time_closest_to_ball { get; set; }
        public float? Time_farthest_from_ball { get; set; }
        public float? Percent_defensive_third { get; set; }
        public float? Percent_offensive_third { get; set; }
        public float? Percent_neutral_third { get; set; }
        public float? Percent_defensive_half { get; set; }
        public float? Percent_offensive_half { get; set; }
        public float? Percent_behind_ball { get; set; }
        public float? Percent_infront_ball { get; set; }
        public float? Percent_most_back { get; set; }
        public float? Percent_most_forward { get; set; }
        public float? Percent_closest_to_ball { get; set; }
        public float? Percent_farthest_from_ball { get; set; }
        public int? Goals_against_while_last_defender { get; set; }
    }
}
