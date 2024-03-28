namespace BallchasingWrapper.Models.ReplayModels.Advanced
{
    public class PlayerPositioning : GeneralPositioning
    {
        [JsonProperty("avg_distance_to_ball")]
        public int AvgDistanceToBall { get; set; }
        [JsonProperty("avg_distance_to_ball_possession")]
        public int AvgDistanceToBallPossession { get; set; }
        [JsonProperty("avg_distance_to_ball_no_possession")]
        public int AvgDistanceToBallNoPossession { get; set; }
        [JsonProperty("avg_distance_to_mates")]
        public int AvgDistanceToMates { get; set; }
        [JsonProperty("time_most_back")]
        public float TimeMostBack { get; set; }
        [JsonProperty("time_most_forward")]
        public float TimeMostForward { get; set; }
        [JsonProperty("time_closest_to_ball")]
        public float TimeClosestToBall { get; set; }
        [JsonProperty("time_farthest_from_ball")]
        public float TimeFarthestFromBall { get; set; }
        [JsonProperty("percent_defensive_third")]
        public float PercentDefensiveThird { get; set; }
        [JsonProperty("percent_offensive_third")]
        public float PercentOffensiveThird { get; set; }
        [JsonProperty("percent_neutral_third")]
        public float PercentNeutralThird { get; set; }
        [JsonProperty("percent_defensive_half")]
        public float PercentDefensiveHalf { get; set; }
        [JsonProperty("percent_offensive_half")]
        public float PercentOffensiveHalf { get; set; }
        [JsonProperty("percent_behind_ball")]
        public float PercentBehindBall { get; set; }
        [JsonProperty("percent_infront_ball")]
        public float PercentInfrontBall { get; set; }
        [JsonProperty("percent_most_back")]
        public float PercentMostBack { get; set; }
        [JsonProperty("percent_most_forward")]
        public float PercentMostForward { get; set; }
        [JsonProperty("percent_closest_to_ball")]
        public float PercentClosestToBall { get; set; }
        [JsonProperty("percent_farthest_from_ball")]
        public float PercentFarthestFromBall { get; set; }
        [JsonProperty("goals_against_while_last_defender")]
        public int GoalsAgainstWhileLastDefender { get; set; }
    }
}
