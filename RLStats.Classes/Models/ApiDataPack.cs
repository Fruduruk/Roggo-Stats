global using Newtonsoft.Json;

using RLStats_Classes.Models.ReplayModels;

using System.Collections.Generic;

namespace RLStats_Classes.Models
{
    public class ApiDataPack
    {
        [JsonProperty("list")]
        public List<Replay> Replays { get; set; }

        [JsonProperty("count")]
        public int ReplayCount { get; set; }

        [JsonProperty("next")]
        public string Next { get; set; }


        public bool Success { get; set; }
        public string ReceivedString { get; set; } = string.Empty;
    }
}
