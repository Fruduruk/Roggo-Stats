namespace BallchasingWrapper.Models.ReplayModels.Advanced
{
    public class PlayerCore : GeneralCore
    {
        [JsonProperty("mvp")]
        public bool Mvp { get; set; }
    }
}
