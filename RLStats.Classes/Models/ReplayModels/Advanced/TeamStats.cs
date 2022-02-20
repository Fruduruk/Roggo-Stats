namespace RLStats_Classes.Models.ReplayModels.Advanced
{
    public class TeamStats
    {
        [JsonProperty("ball")]
        public Ball Ball { get; set; }

        [JsonProperty("core")]
        public Core Core { get; set; }

        [JsonProperty("boost")]
        public Boost Boost { get; set; }

        [JsonProperty("movement")]
        public GeneralMovement Movement { get; set; }

        [JsonProperty("positioning")]
        public GeneralPositioning Positioning { get; set; }

        [JsonProperty("demo")]
        public Demo Demo { get; set; }
    }
}