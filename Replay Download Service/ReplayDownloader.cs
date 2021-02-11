using Microsoft.Extensions.Logging;
using RLStats_Classes.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Replay_Download_Service
{
    internal class ReplayDownloader
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
                    SInfo = new ServiceInfoReader().GetServiceInfo();
                    Logger.LogInformation($"Work started: {DateTime.Now}");
                    DoWork();
                }
            }, stoppingToken);
        }

        private static void DoWork()
        {

        }
    }
}