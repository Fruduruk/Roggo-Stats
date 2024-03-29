namespace BallchasingWrapper.Models.ReplayModels
{
    public class Uploader
    {
        [JsonProperty("steam_id")] public string SteamId { get; set; } = string.Empty;
        [JsonProperty("name")] public string Name { get; set; } = string.Empty;
        [JsonProperty("profile_url")] public string ProfileUrl { get; set; } = string.Empty;
        [JsonProperty("avatar")] public string Avatar { get; set; } = string.Empty;
    }
}