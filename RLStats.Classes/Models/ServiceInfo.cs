
using System.Collections.Generic;

namespace RLStatsClasses.Models
{
    public class ServiceInfo
    {
        public ServiceTokenInfo TokenInfo { get; set; }

        public IList<APIRequestFilter> Filters { get; set; } = new List<APIRequestFilter>();

        public double CycleIntervalInHours { get; set; }

        [JsonIgnore]
        public bool Available { get; set; } = true;
    }
}
