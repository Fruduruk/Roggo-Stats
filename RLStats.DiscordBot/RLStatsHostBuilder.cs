﻿using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;

using Discord_Bot.Configuration;
using Discord_Bot.CustomLogging;
using Discord_Bot.Services;
using Discord_Bot.Singletons;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using MongoDB.Driver;

using RLStats.MongoDBSupport;

using RLStatsClasses.CacheHandlers;
using RLStatsClasses.Interfaces;

using System;
using System.IO;

namespace Discord_Bot
{
    public class RLStatsHostBuilder
    {
        private readonly IHostBuilder _builder;

        public RLStatsHostBuilder()
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration(x =>
                {
                    var configuration = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", false, true)
                        .Build();

                    x.AddConfiguration(configuration);
                })
                .ConfigureLogging(x =>
                {
                    x.AddSimpleConsole();
                    x.SetMinimumLevel(LogLevel.Debug); // Defines what kind of information should be logged (e.g. Debug, Information, Warning, Critical) adjust this to your liking
                    x.AddProvider(new FileLoggingProvider(new FileLoggerConfiguration(LogLevel.Debug)));
                })
                .ConfigureDiscordHost((context, config) =>
                {
                    config.SocketConfig = new DiscordSocketConfig
                    {
                        LogLevel = LogSeverity.Verbose, // Defines what kind of information should be logged from the API (e.g. Verbose, Info, Warning, Critical) adjust this to your liking
                        AlwaysDownloadUsers = true,
                        MessageCacheSize = 200,
                    };

                    config.Token = context.Configuration["token"];
                })
                .UseCommandService((context, config) =>
                {
                    config.CaseSensitiveCommands = false;
                    config.LogLevel = LogSeverity.Verbose;
                    config.DefaultRunMode = RunMode.Async;
                    config.ThrowOnError = true;
                })
                .ConfigureServices((context, services) =>
                {
                    if (context.Configuration["DB"].Equals("MongoDB"))
                    {
                        var settings = new MongoClientSettings
                        {
                            Server = new MongoServerAddress(context.Configuration["Host"], Convert.ToInt32(context.Configuration["Port"]))
                        };
                        var username = context.Configuration["Username"];
                        var password = context.Configuration["Password"];
                        if (username != null && password != null)
                            settings.Credential = MongoCredential.CreateCredential(context.Configuration["DatabaseName"], username, password);
                        var db = new RLStatsDiscordBotMongoDatabase(new DatabaseSettings
                        {
                            MongoSettings = settings,
                            DatabaseName = context.Configuration["DatabaseName"]
                        });
                        services.AddSingleton<IDatabase>(db);
                        services.AddSingleton<IReplayCache>(db);
                        services.AddSingleton<IConfigHandler<Subscription>>(db);
                        services.AddSingleton<IConfigHandler<UserFavorite>>(db);
                    }
                    else
                    {
                        services.AddSingleton<IDatabase>(new Database());
                        services.AddSingleton<IReplayCache>(new ReplayCache());
                        services.AddSingleton<IConfigHandler<Subscription>>(new ConfigHandler<Subscription>(Constants.SubscribtionConfigFilePath));
                        services.AddSingleton<IConfigHandler<UserFavorite>>(new ConfigHandler<UserFavorite>(Constants.UserFavoritesConfigFilePath));
                    }

                    services.AddSingleton(context.Configuration["ballchasing-token"]);
                    services.AddSingleton(new RecentlyAddedEntries());
                    services.AddSingleton(new CommandsToProceed());

                    services.AddHostedService<CommandHandler>();
                    services.AddHostedService<RecurringReportsService>();
                })
                .UseConsoleLifetime();
            _builder = builder;
        }

        public IHost GetHost()
        {
            return _builder.Build();
        }
    }
}
