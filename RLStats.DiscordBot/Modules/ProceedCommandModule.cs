using Discord.Commands;

using Discord_Bot.Singletons;

using System;
using System.Threading.Tasks;

namespace Discord_Bot.Modules
{
    public class ProceedCommandModule : ModuleBase<SocketCommandContext>
    {
        private readonly IServiceProvider _provider;
        private readonly CommandService _service;
        private readonly CommandsToProceed _commandsToProceed;
        public ProceedCommandModule(IServiceProvider provider, CommandService service, CommandsToProceed commandsToProceed)
        {
            _provider = provider;
            _service = service;
            _commandsToProceed = commandsToProceed;
        }


        [Command("proceed")]
        [Alias("p")]
        public async Task ProceedCommand(string command)
        {
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
