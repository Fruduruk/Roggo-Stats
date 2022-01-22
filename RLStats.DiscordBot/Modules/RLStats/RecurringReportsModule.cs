using Discord.Commands;

using Microsoft.Extensions.Logging;

using System.Threading.Tasks;

namespace Discord_Bot.Modules.RLStats
{
    //[Remarks("HelperModule")]
    [Name("Subscription Commands")]
    public class RecurringReportsModule : RlStatsModuleBase
    {
        public RecurringReportsModule(ILogger<RecurringReportsModule> logger, string ballchasingToken) : base(logger, ballchasingToken)
        {
            
        }

        [Command("subscribe")]
        [Summary("This command lets you subscribe a specific action")]
        public async Task Subscribe(params string[] args)
        {
            int i = 0;
        }
    }
}
