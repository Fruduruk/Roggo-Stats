using RLStatsClasses.Models;

namespace RLStatsClasses.Interfaces
{
    public interface IServiceInfoIO
    {
        ServiceInfo GetServiceInfo();
        void SaveServiceInfo(ServiceInfo info);
    }
}
