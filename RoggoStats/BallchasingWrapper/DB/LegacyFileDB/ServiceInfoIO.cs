using BallchasingWrapper.BusinessLogic;
using BallchasingWrapper.Interfaces;
using BallchasingWrapper.Models;

namespace BallchasingWrapper.DB.LegacyFileDB
{
    public class ServiceInfoIO : IServiceInfoIO
    {
        private string ServiceFilePath { get; set; }
        public ServiceInfoIO()
        {
            ServiceFilePath = Path.Combine(RLConstants.RLStatsFolder, @"serviceInfo");
            if (!Directory.Exists(RLConstants.RLStatsFolder))
                Directory.CreateDirectory(RLConstants.RLStatsFolder);
            if (!File.Exists(ServiceFilePath))
                File.Create(ServiceFilePath).Dispose();
        }
        public ServiceInfo GetServiceInfo()
        {
            try
            {
                var info = Compressor.ConvertObject<ServiceInfo>(File.ReadAllBytes(ServiceFilePath));
                return info;
            }
            catch
            {
                return new ServiceInfo { Available = false };
            }
        }
        public void SaveServiceInfo(ServiceInfo info)
        {
            File.WriteAllBytes(ServiceFilePath, Compressor.ConvertObject(info));
        }
    }
}