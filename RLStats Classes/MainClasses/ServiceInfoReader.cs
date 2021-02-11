using Newtonsoft.Json;
using RLStats_Classes.Models;
using System;
using System.IO;

namespace RLStats_Classes.MainClasses
{
    public class ServiceInfoReader
    {
        private string ServiceDirectory { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\RocketLeagueStatsService";
        private string ServiceFilePath { get; set; }
        public ServiceInfoReader()
        {
            ServiceFilePath = ServiceDirectory + @"\serviceInfo.txt";
            if (!Directory.Exists(ServiceDirectory))
                Directory.CreateDirectory(ServiceDirectory);
            if (!File.Exists(ServiceFilePath))
                File.Create(ServiceFilePath);
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
    }
}