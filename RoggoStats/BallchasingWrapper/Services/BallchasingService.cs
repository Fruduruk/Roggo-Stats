using BallchasingWrapper.Grpc;
using Grpc.Core;

namespace BallchasingWrapper.Services;

public class BallchasingService : Grpc.Ballchasing.BallchasingBase
{
    private readonly ILogger<BallchasingService> _logger;

    public BallchasingService(ILogger<BallchasingService> logger)
    {
        _logger = logger;
    }

    public override Task<SimpleReplaysResponse> GetSimpleReplays(RequestFilter request, ServerCallContext context)
    {
        return Task.FromResult(new SimpleReplaysResponse
        {
            Replays = { new Replay
            {
                Orange = new Team
                {
                    Players = { new Player
                    {
                        Score = 0
                    } }
                }
            } }
        });
    }
}