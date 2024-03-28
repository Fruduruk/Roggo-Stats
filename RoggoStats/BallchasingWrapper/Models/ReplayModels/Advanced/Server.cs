namespace BallchasingWrapper.Models.ReplayModels.Advanced;

public class Server
{
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
    [JsonProperty("region")] public string Region { get; set; } = string.Empty;
}