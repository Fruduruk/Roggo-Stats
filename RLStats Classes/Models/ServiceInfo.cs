using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RLStats_Classes.Models
{
    public class ServiceInfo
    {
        public string Token { get; set; }
        public List<string> Names { get; set; }
        public List<string> SteamIDs { get; set; }
        [JsonIgnore]
        public bool Available { get; set; } = true;

    }
}
