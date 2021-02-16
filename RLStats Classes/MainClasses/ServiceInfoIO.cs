using Newtonsoft.Json;
using RLStats_Classes.Models;
using System;
using System.IO;

namespace RLStats_Classes.MainClasses
{
    public class ServiceInfoIO
    {
        private string ServiceDirectory { get; set; } = RLConstants.RLStatsFolder + @"\RocketLeagueStatsService";
        private string ServiceFilePath { get; set; }
        public ServiceInfoIO()
        {
            ServiceFilePath = ServiceDirectory + @"\serviceInfo.json";
            if (!Directory.Exists(ServiceDirectory))
                Directory.CreateDirectory(ServiceDirectory);
            if (!File.Exists(ServiceFilePath))
                File.Create(ServiceFilePath).Dispose();
        }
        public ServiceInfo GetServiceInfo()
        {
            var info = new ServiceInfo();
            var text = File.ReadAllText(ServiceFilePath);
            if (text.Equals(string.Empty))
            {
                info.Available = false;
                return info;
            }
            else
            {
                info = JsonConvert.DeserializeObject<ServiceInfo>(text);
                return info;
            }
        }
        public void SaveServiceInfo(ServiceInfo info)
        {
            var jsonString = JsonConvert.SerializeObject(info);
            File.WriteAllText(ServiceFilePath, jsonString);
        }
    }
}