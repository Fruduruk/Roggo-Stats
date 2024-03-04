namespace RLStatsClasses.Models.ReplayModels.Advanced
{
    public class PlayerStats
    {
        [JsonProperty("core")]
        public PlayerCore PlayerCore { get; set; }

        [JsonProperty("boost")]
        public PlayerBoost PlayerBoost { get; set; }

        [JsonProperty("movement")]
        public PlayerMovement PlayerMovement { get; set; }

        [JsonProperty("positioning")]
        public PlayerPositioning PlayerPositioning { get; set; }

        [JsonProperty("demo")]
        public Demo Demo { get; set; }
    }
}

