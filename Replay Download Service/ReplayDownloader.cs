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
        private static ServiceInfo SInfo { get; set; }
        internal static Task ExecuteServiceAsync(ILogger<Worker> logger, CancellationToken stoppingToken)
        {
            return Task.Run(() =>
            {
                Logger = logger;
                StoppingToken = stoppingToken;
                while (!stoppingToken.IsCancellationRequested)
                {
                    SInfo = new ServiceInfoIO().GetServiceInfo();
                    Logger.LogInformation($"Work started: {DateTime.Now}");
                    StartDownloadCycle();
                }
            }, stoppingToken);
        }

        private static void StartDownloadCycle()
        {
            var s = string.Empty;
            foreach (var f in SInfo.Filters)
                s += f.GetApiUrl() + "\n";
            Logger.LogInformation(s);
            Thread.Sleep(5234);
        }
    }
}