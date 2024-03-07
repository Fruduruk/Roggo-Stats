using BallchasingWrapper.BusinessLogic;

namespace BallchasingWrapper.Models
{
    public class ServiceInfo
    {
        public ServiceTokenInfo TokenInfo { get; set; }

        public IList<ApiUrlCreator> Filters { get; set; } = new List<ApiUrlCreator>();

        public double CycleIntervalInHours { get; set; }

        [JsonIgnore]
        public bool Available { get; set; } = true;
    }
}
