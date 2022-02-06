using Discord.Commands;

using Discord_Bot.Singletons;

using Microsoft.Extensions.Logging;

using System;
using System.Threading.Tasks;

namespace Discord_Bot.Modules
{
    public class ProceedCommandModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<ProceedCommandModule> _logger;
        private readonly IServiceProvider _provider;
        private readonly CommandService _service;
        private readonly CommandsToProceed _commandsToProceed;
        public ProceedCommandModule(ILogger<ProceedCommandModule> logger,IServiceProvider provider, CommandService service, CommandsToProceed commandsToProceed)
        {
            _logger = logger;
            _provider = provider;
            _service = service;
            _commandsToProceed = commandsToProceed;
        }


        [Command("proceed")]
        [Alias("p")]
        public async Task ProceedCommand(string command)
        {
            _logger.LogInformation($"Proceeding command: {command}");
            foreach (var commandInProgress in _commandsToProceed.CommandsInProgress)
            {
                if (commandInProgress.ChannelId == Context.Channel.Id && commandInProgress.UserId == Context.User.Id)
                {
                    await _service.ExecuteAsync(Context, $"{commandInProgress.CommandToProceed} {command}", _provider);
                }
            }
        }
    }
}
