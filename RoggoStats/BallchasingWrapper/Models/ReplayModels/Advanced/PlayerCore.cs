namespace BallchasingWrapper.Models.ReplayModels.Advanced
{
    public class PlayerCore : Core
    {
        [JsonProperty("mvp")]
        public bool Mvp { get; set; }
    }
}
