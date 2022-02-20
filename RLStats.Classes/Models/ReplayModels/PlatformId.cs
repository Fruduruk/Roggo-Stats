namespace RLStatsClasses.Models.ReplayModels
{
    public class PlatformId
    {
        [JsonProperty("platform")]
        public string Platform { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }
}