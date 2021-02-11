using RLStats_Classes.MainClasses;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RLStats_Classes.Models
{
    public class ServiceInfo
    {
        public string Token { get; set; }
        public IList<APIRequestFilter> Filters { get; set; } = new List<APIRequestFilter>();
        [JsonIgnore]
        public bool Available { get; set; } = true;

    }
}
