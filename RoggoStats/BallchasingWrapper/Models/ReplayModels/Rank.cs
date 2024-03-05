namespace BallchasingWrapper.Models.ReplayModels
{
    public class Rank
    {
        [JsonProperty("tier")]
        public int Tier { get; set; }

        [JsonProperty("division")]
        public int Division { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public string? Id { get; set; }
    }
}