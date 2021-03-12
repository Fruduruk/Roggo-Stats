using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RLStats_Classes.MainClasses;
using RLStats_Classes.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Replay_Download_Service
{
    public class ReplayDownloader
    {
        private static ILogger<Worker> Logger { get; set; }
        private static CancellationToken StoppingToken { get; set; }
        internal static Task ExecuteServiceAsync(ILogger<Worker> logger, CancellationToken stoppingToken)
        {
            return Task.Run(() =>
            {
                Logger = logger;
                StoppingToken = stoppingToken;
                while (!stoppingToken.IsCancellationRequested)
                {
                    var sInfo = new ServiceInfoIO().GetServiceInfo();
                    Logger.LogInformation($"Work started: {DateTime.Now}");
                    StartDownloadCycle(sInfo);
                    Thread.Sleep(1000);
                }
            }, stoppingToken);
        }

        private static void StartDownloadCycle(ServiceInfo serviceInfo)
        {
            if (!serviceInfo.Available)
            {
                Logger.LogInformation("Service is not available. Cannot get information out of serice info file");
                Thread.Sleep(10000);
                return;
            }

            var connection = new Connection(serviceInfo.TokenInfo);
        }
    }
}