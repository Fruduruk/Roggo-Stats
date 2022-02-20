namespace RLStats_Classes.Models.ReplayModels
{
    public class Uploader
    {
        [JsonProperty("steam_id")]
        public string SteamId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("profile_url")]
        public string ProfileUrl { get; set; }

        [JsonProperty("avatar")]
        public string Avatar { get; set; }
    }
}