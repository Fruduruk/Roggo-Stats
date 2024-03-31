global using Newtonsoft.Json;
using BallchasingWrapper.BusinessLogic;
using BallchasingWrapper.DB.MongoDB;
using BallchasingWrapper.Interfaces;
using BallchasingWrapper.Models;
using BallchasingWrapper.Services;

namespace BallchasingWrapper
{
    public static class Program
    {
        private const string BallchasingApiKey = "BALLCHASING_API_KEY";
        private const string MongoHost = "MONGO_HOST";
        private const string MongoPort = "MONGO_PORT";
        private const string DatabaseName = "DATABASE_NAME";
        private const string MongoUser = "MONGO_USER";
        private const string MongoPassword = "MONGO_PASSWORD";
        private const string AuthenticationDatabaseName = "admin";

        private static AuthTokenInfo? _tokenInfo;

        public static void Main(string[] args)
        {
            if (IsAnyEnvironmentVariableMissing())
                return;

            var builder = WebApplication.CreateBuilder(args);
            
            Console.WriteLine("Connecting to MongoDb...");
            var mongoDb = SetupMongoDb();
            Console.WriteLine("MongoDb connected!");
            var ballchasingApi = new BallchasingApi(_tokenInfo);
            
            builder.Services.AddGrpc();
            builder.Services.AddSingleton<IReplayCache>(mongoDb);
            builder.Services.AddSingleton<IDatabase>(mongoDb);
            builder.Services.AddSingleton<IBackgroundDownloaderConfig>(mongoDb);
            builder.Services.AddSingleton<IBallchasingApi>(ballchasingApi);
            builder.Services.AddSingleton(mongoDb.ToLoggerProvider());
            builder.Services.AddSingleton<BackgroundDownloadingService>();
            
            var app = builder.Build();
            var logger = app.Services.GetService<ILogger<WebApplication>>();
            app.StartBackgroundDownloader(logger);
            app.MapGrpcService<BallchasingService>();
            app.MapGet("/",
                () =>
                    "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            
            app.Run();
            logger?.LogInformation("Replay collector shutdown successful.");
        }

        private static void StartBackgroundDownloader(this WebApplication app, ILogger? logger)
        {
            var service = app.Services.GetService<BackgroundDownloadingService>();
            if (service is null)
                logger?.LogError("Failed to start Background Download.");
            else
                service.StartBackgroundDownload();
        }

        private static bool IsAnyEnvironmentVariableMissing()
        {
            Console.WriteLine("Hi, this is the Ballchasing Wrapper for Roggo Stats!");
            Console.WriteLine("It is the main Interface that controls the access to the Ballchasing API.");
            Console.WriteLine("Your access to the data ist via gRPC.");

            if (!IsValidBallchasingKey())
            {
                Console.WriteLine(
                    "For this service to work you need to give it a Ballchasing API-Key. This key controls the call limit and can be generated here: https://ballchasing.com/upload");
                Console.WriteLine(
                    $"To set the API-Key you need to set the environment variable '{BallchasingApiKey}'.");
                return true;
            }

            Console.WriteLine("This service uses MongoDB as data caching to minimize the call count.");
            return IsAnyMongoConfigMissing();
        }

        private static bool IsValidBallchasingKey()
        {
            var key = Environment.GetEnvironmentVariable(BallchasingApiKey);
            if (key is null)
            {
                Console.WriteLine($"{BallchasingApiKey} missing.");
                return false;
            }

            var tokenInfo = TokenInfoProvider.GetTokenInfo(key);
            if (tokenInfo.Except is not null)
            {
                Console.WriteLine(tokenInfo.Except.Message);
                return false;
            }

            if (!tokenInfo.Chaser)
            {
                Console.WriteLine("You are not a chaser. I can't let you in.");
                return false;
            }

            _tokenInfo = tokenInfo;
            return true;
        }

        private static bool IsAnyMongoConfigMissing()
        {
            var hostAvailable = IsEnvironmentVariableAvailable(MongoHost);
            var portAvailable = IsEnvironmentVariableAvailable(MongoPort);
            var userAvailable = IsEnvironmentVariableAvailable(MongoUser);
            var passwordAvailable = IsEnvironmentVariableAvailable(MongoPassword);
            var databaseNameAvailable = IsEnvironmentVariableAvailable(DatabaseName);
            return !(hostAvailable && portAvailable && userAvailable && passwordAvailable && databaseNameAvailable);
        }

        private static bool IsEnvironmentVariableAvailable(string variable)
        {
            if (Environment.GetEnvironmentVariable(variable) is not null) return true;
            Console.WriteLine($"{variable} missing.");
            return false;
        }

        private static RlStatsMongoDatabase SetupMongoDb()
        {
            var host = Environment.GetEnvironmentVariable(MongoHost);
            var port = Convert.ToInt32(Environment.GetEnvironmentVariable(MongoPort));
            var settings = new DatabaseSettings
            {
                MongoSettings = new MongoDB.Driver.MongoClientSettings
                {
                    Server = new MongoDB.Driver.MongoServerAddress(host, port),
                    Compressors = new[]
                    {
                        new MongoDB.Driver.Core.Configuration.CompressorConfiguration(MongoDB.Driver.Core
                            .Compression.CompressorType.Snappy)
                    },
                    ServerSelectionTimeout = TimeSpan.FromSeconds(3)
                },
                DatabaseName = Environment.GetEnvironmentVariable(DatabaseName)
            };
            var username = Environment.GetEnvironmentVariable(MongoUser);
            var password = Environment.GetEnvironmentVariable(MongoPassword);
            if (username != null && password != null)
            {
                settings.MongoSettings.Credential =
                    MongoDB.Driver.MongoCredential.CreateCredential(AuthenticationDatabaseName, username, password);
            }

            return new RlStatsMongoDatabase(settings);
        }
    }
}