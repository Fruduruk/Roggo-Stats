namespace BallchasingWrapper.Models.ReplayModels
{
    public class PlatformId
    {
        [JsonProperty("platform")] public string Platform { get; set; } = string.Empty;
        [JsonProperty("id")] public string Id { get; set; } = string.Empty;
    }
}