namespace BallchasingWrapper.Models.ReplayModels.Advanced
{
    public class AdvancedPlayer
    {
        [JsonProperty("start_time")] public float StartTime { get; set; }
        [JsonProperty("end_time")] public float EndTime { get; set; }
        [JsonProperty("name")] public string Name { get; set; } = string.Empty;
        [JsonProperty("id")] public PlatformId Id { get; set; } = new();
        [JsonProperty("rank")] public Rank Rank { get; set; } = new();
        [JsonProperty("car_id")] public int CarId { get; set; }
        [JsonProperty("car_name")] public string CarName { get; set; } = string.Empty;
        [JsonProperty("camera")] public Camera Camera { get; set; } = new();
        [JsonProperty("steering_sensitivity")] public double SteeringSensitivity { get; set; }
        [JsonProperty("stats")] public PlayerStats Stats { get; set; } = new();
    }
}