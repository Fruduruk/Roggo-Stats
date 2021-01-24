using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Replay_Download_Service
{
    internal class ReplayDownloader
    {
        private static ILogger<Worker> Logger { get; set; }
        private static CancellationToken StoppingToken { get; set; }
        internal static Task ExecuteServiceAsync(ILogger<Worker> logger, CancellationToken stoppingToken)
        {
            Logger = logger;
            StoppingToken = stoppingToken;
            return null;
        }
    }
}