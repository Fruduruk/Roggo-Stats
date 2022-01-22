using Microsoft.Extensions.Hosting;

using System.Threading.Tasks;

namespace Discord_Bot
{
    class Program
    {
        static async Task Main()
        {
            var hostBuilder = new RLStatsHostBuilder();
            using var host = hostBuilder.GetHost();
            await host.RunAsync();
        }
    }
}