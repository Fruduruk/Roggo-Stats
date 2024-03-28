namespace BallchasingWrapper.Models.ReplayModels.Advanced
{
    public class Camera
    {
        [JsonProperty("fov")] public int Fov { get; set; }
        [JsonProperty("height")] public int Height { get; set; }
        [JsonProperty("pitch")] public int Pitch { get; set; }
        [JsonProperty("distance")] public int Distance { get; set; }
        [JsonProperty("stiffness")] public float Stiffness { get; set; }
        [JsonProperty("swivel_speed")] public float SwivelSpeed { get; set; }
        [JsonProperty("transition_speed")] public float TransitionSpeed { get; set; }
    }
}