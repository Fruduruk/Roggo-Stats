using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static Discord_Bot.Modules.RLStats.RecurringReports.RecurringReportsConstants;

namespace Discord_Bot.Services
{
    public class CommandHandler : DiscordClientService
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _service;
        private readonly IConfiguration _config;

        public CommandHandler(IServiceProvider provider, DiscordSocketClient client, ILogger<DiscordClientService> logger, CommandService service, IConfiguration config) : base(client, logger)
        {
            _provider = provider;
            _client = client;
            _service = service;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _client.MessageReceived += OnMessageReceived;

            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);

            await Client.SetStatusAsync(UserStatus.Invisible);
        }

        private async Task OnMessageReceived(SocketMessage arg)
        {
            //Check if message comes from a user
            if (!(arg is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            //if (message.Reference != null)
            //{
            // Maybe someday replying is the way to go
            //}

            //Check if this user wants to call a command and execute it
            var argPos = 0;
            if (!message.HasStringPrefix(_config["prefix"], ref argPos) && !message.HasMentionPrefix(_client.CurrentUser, ref argPos)) return;

            var context = new SocketCommandContext(_client, message);

            if (message.ToString().StartsWith($"{_config["prefix"]}help"))
            {
                await Help(context);
                return;
            }
            await _service.ExecuteAsync(context, argPos, _provider);
        }

        public async Task Help(ICommandContext context)
        {
            await ShowTitle(context);
            await ShowParameter(context);
            await ShowCommands(context);
        }

        private async Task ShowCommands(ICommandContext context)
        {
            foreach (var module in _service.Modules)
            {
                if (!"HelperModule".Equals(module.Remarks))
                    continue;
                var builder = new EmbedBuilder()
                .WithColor(new Color(127, 127, 255))
                .WithTitle(module.Name);
                foreach (var command in module.Commands)
                {
                    if (ProceedingMethod.Equals(command.Remarks))
                        continue;
                    var value = command.Summary ?? "This command has no summary" + Environment.NewLine;
                    value += $"\n{_config["prefix"]}{command.Name}";
                    foreach (var param in command.Parameters)
                    {
                        value += " {" + param.Name + "}";
                    }
                    builder.AddField(_config["prefix"] + command.Name, value);
                }
                if (module.Commands.Count > 0)
                    await context.Channel.SendMessageAsync(null, false, builder.Build());
            }
        }

        private static async Task ShowParameter(ICommandContext context)
        {
            var paramsEmbedBuilder = new EmbedBuilder()
                            .WithColor(new Color(0, 255, 0))
                            .WithTitle("Parameters")
                            .WithDescription("Here is how to use the parameters")
                            .AddField(new EmbedFieldBuilder().WithName("time").WithValue("y = year, m = month, w = week, d = day"))
                            .AddField(new EmbedFieldBuilder().WithName("together").WithValue("y = yes, n = no"))
                            .AddField(new EmbedFieldBuilder().WithName("names").WithValue("a list of names or steam ids separated by whitespaces"));

            await context.Channel.SendMessageAsync(null, false, paramsEmbedBuilder.Build());
        }

        private static async Task ShowTitle(ICommandContext context)
        {
            var titleEmbedBuilder = new EmbedBuilder()
                            .WithColor(new Color(255, 0, 0))
                            .WithTitle("Rocket League Stats Discord Bot")
                            .WithDescription("This bot collects data from ballchasing.com to calculate average stats.")
                            .WithUrl("https://ballchasing.com")
                            .WithFooter(new EmbedFooterBuilder().WithText("A bot by Fruduruk").WithIconUrl("https://cdn.cloudflare.steamstatic.com/steamcommunity/public/images/avatars/36/360236e555049f204b12d3a8685a3b9b9764ebfe_full.jpg"))

                            .AddField(new EmbedFieldBuilder().WithName("-stats").WithValue("Core stats like goals, assists and saves"))
                            .AddField(new EmbedFieldBuilder().WithName("-boost").WithValue("Boost stats like boost per minute and pads collected"))
                            .AddField(new EmbedFieldBuilder().WithName("-mov").WithValue("Movement stats like average speed and percentage supersonic speed"))
                            .AddField(new EmbedFieldBuilder().WithName("-pos").WithValue("Positioning stats like time defensive third and time behind ball"))
                            .AddField(new EmbedFieldBuilder().WithName("-demo").WithValue("Demos taken and inflicted"));

            await context.Channel.SendMessageAsync(null, false, titleEmbedBuilder.Build());
        }

        
    }
}