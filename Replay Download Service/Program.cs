using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using RLStats.MongoDBSupport;

using System.IO;

namespace ReplayDownloadService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", false, true)
                        .Build();
            var dbType = configuration.GetSection("DB").Value;

            switch (dbType)
            {
                case "MongoDB":
                    DBProvider.CreateInstance(DBType.MongoDB, new DatabaseSettings
                    {
                        ConnectionString = "mongodb://localhost:27017",
                        DatabaseName = "RLStatsData"
                    });
                    break;
                default:
                    DBProvider.CreateInstance(DBType.Legacy);
                    break;
            }
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService(config =>
                {
                    config.ServiceName = "Rocket League Stats Replay Downloader";
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();


                    services.AddSingleton(typeof(DBProvider), DBProvider.Instance);
                });
    }
}
