using BallchasingWrapper.Models;

namespace BallchasingWrapper.Interfaces
{
    public interface IServiceInfoIO
    {
        ServiceInfo GetServiceInfo();
        void SaveServiceInfo(ServiceInfo info);
    }
}
