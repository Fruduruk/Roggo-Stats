using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using RLStats.MongoDBSupport;

using System;
using System.IO;
using System.Linq;

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
                case "Legacy":
                    DBProvider.CreateInstance(DBType.Legacy);
                    break;
                default: throw new Exception("Database not specified. Options are Legacy and MongoDB");
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
