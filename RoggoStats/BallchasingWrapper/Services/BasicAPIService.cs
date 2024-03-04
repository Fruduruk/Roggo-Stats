using Grpc.Core;
using BallchasingWrapper;

namespace BallchasingWrapper.Services;

public class BasicAPIService : BasicAPI.BasicAPIBase
{
    private readonly ILogger<BasicAPIService> _logger;

    public BasicAPIService(ILogger<BasicAPIService> logger)
    {
        _logger = logger;
    }

    public override Task<SimpleReplayReply> GetSimpleReplay(SimpleReplayRequest request, ServerCallContext context)
    {
        return Task.FromResult(new SimpleReplayReply
        {
            Title = "Id: "+request.Id
        });
    }
}