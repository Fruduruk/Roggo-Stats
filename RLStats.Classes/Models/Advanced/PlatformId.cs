namespace RLStats_Classes.Models.Advanced
{
    public class PlatformId
    {
        [JsonProperty("platform")]
        public string Platform { get; set; }

        [JsonProperty("id")]
        public string ID { get; set; }
    }
}