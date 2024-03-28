namespace BallchasingWrapper.Models.ReplayModels.Advanced
{
    public class Demo
    {
        [JsonProperty("inflicted")]
        public int Inflicted { get; set; }
        [JsonProperty("taken")]
        public int Taken { get; set; }
    }
}