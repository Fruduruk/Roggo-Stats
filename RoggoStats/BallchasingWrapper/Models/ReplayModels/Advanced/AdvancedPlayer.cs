namespace BallchasingWrapper.Models.ReplayModels.Advanced
{
    public class AdvancedPlayer
    {
        [JsonProperty("start_time")]
        public float? StartTime { get; set; }

        [JsonProperty("end_time")]
        public float? EndTime { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public PlatformId Id { get; set; }

        [JsonProperty("rank")]
        public Rank Rank { get; set; }

        [JsonProperty("car_id")]
        public int? CarId { get; set; }

        [JsonProperty("car_name")]
        public string Car_name { get; set; }

        [JsonProperty("camera")]
        public Camera Camera { get; set; }

        [JsonProperty("steering_sensitivity")]
        public double? SteeringSensitivity { get; set; }

        [JsonProperty("stats")]
        public PlayerStats Stats { get; set; }
    }
}
