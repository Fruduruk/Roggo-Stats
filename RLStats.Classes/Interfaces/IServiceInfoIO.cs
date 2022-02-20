using RLStats_Classes.Models;

namespace RLStats_Classes.Interfaces
{
    public interface IServiceInfoIO
    {
        ServiceInfo GetServiceInfo();
        void SaveServiceInfo(ServiceInfo info);
    }
}
