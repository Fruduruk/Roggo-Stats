using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ReplayDownloadService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly DBProvider _dBProvider;

        public Worker(ILogger<Worker> logger, DBProvider dBProvider)
        {
            _logger = logger;
            _dBProvider = dBProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Replay Downloader Service started at: {time}", DateTimeOffset.Now);
            await ReplayDownloader.ExecuteServiceAsync(_dBProvider, _logger, stoppingToken);
        }
    }
}
