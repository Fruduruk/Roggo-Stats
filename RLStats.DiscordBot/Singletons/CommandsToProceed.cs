using System.Collections.Generic;

namespace Discord_Bot.Singletons
{
    public class CommandsToProceed
    {
        public List<CommandInProgress> CommandsInProgress { get; set; } = new List<CommandInProgress>();
    }
}
