
using System.Collections.Generic;

namespace RLStats_Classes.Models.ReplayModels.Advanced
{
    public class AdvancedTeam
    {
        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("players")]
        public List<AdvancedPlayer> Players { get; set; }

        [JsonProperty("stats")]
        public TeamStats Stats { get; set; }
    }
}

