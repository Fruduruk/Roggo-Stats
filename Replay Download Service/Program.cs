using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using RLStats.MongoDBSupport;

using RLStatsClasses;

using System;
using System.IO;
using System.Reflection;

namespace ReplayDownloadService
{
    public class Program
    {
        private const string DatabaseName = "RLStatsData";

        public static void Main(string[] args)
        {
            try
            {
                var installationPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var configuration = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile(Path.Combine(installationPath, "appsettings.json"), false, true)
                            .Build();
                var dbType = configuration.GetSection("DB").Value;

                switch (dbType)
                {
                    case "MongoDB":
                        SetupMongoDB(configuration);
                        break;
                    default:
                        DBProvider.CreateInstance(DBType.Legacy);
                        break;
                }
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                var path = Path.Combine(RLConstants.RLStatsFolder, "ServiceLog.txt");
                File.AppendAllLines(path, new string[] { ex.ToString() });
            }
        }

        private static void SetupMongoDB(IConfigurationRoot configuration)
        {
            var mongoSection = configuration.GetSection("Mongo");
            var host = mongoSection.GetSection("Host").Value;
            var port = Convert.ToInt32(mongoSection.GetSection("Port").Value);
            if (host == null)
                throw new ArgumentNullException("Host was null");
            var settings = new DatabaseSettings
            {
                MongoSettings = new MongoDB.Driver.MongoClientSettings
                {
                    Server = new MongoDB.Driver.MongoServerAddress(host, port),
                    Compressors = new[] { new MongoDB.Driver.Core.Configuration.CompressorConfiguration(MongoDB.Driver.Core.Compression.CompressorType.Snappy) }
                },
                DatabaseName = DatabaseName
            };
            var username = mongoSection.GetSection("Username").Value;
            var password = mongoSection.GetSection("Password").Value;
            if (username != null && password != null)
                settings.MongoSettings.Credential = MongoDB.Driver.MongoCredential.CreateCredential(DatabaseName, username, password);
            DBProvider.CreateInstance(DBType.MongoDB, settings);
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
