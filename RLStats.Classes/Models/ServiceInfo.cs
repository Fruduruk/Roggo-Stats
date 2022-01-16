using RLStats_Classes.MainClasses;

using System.Collections.Generic;

namespace RLStats_Classes.Models
{
    public class ServiceInfo
    {
        public ServiceTokenInfo TokenInfo { get; set; }
        public IList<APIRequestFilter> Filters { get; set; } = new List<APIRequestFilter>();
        public double CycleIntervalInHours { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public bool Available { get; set; } = true;
    }
}
