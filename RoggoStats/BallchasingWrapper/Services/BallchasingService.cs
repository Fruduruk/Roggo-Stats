using BallchasingWrapper.BusinessLogic;
using BallchasingWrapper.DB.MongoDB;
using BallchasingWrapper;
using Google.Protobuf.Collections;
using Grpc.Core;

namespace BallchasingWrapper.Services;

public class BallchasingService : Grpc.Ballchasing.BallchasingBase
{
    private readonly ILogger<BallchasingService> _logger;
    private readonly ReplayProvider _replayProvider;
    private readonly AdvancedReplayProvider _advancedReplayProvider;

    public BallchasingService(ILogger<BallchasingService> logger, BallchasingApi api, RlStatsMongoDatabase db)
    {
        _logger = logger;
        _replayProvider = new ReplayProvider(api, db, logger);
        _advancedReplayProvider = new AdvancedReplayProvider(api, db, logger);
    }

    public override async Task<Grpc.SimpleReplaysResponse> GetSimpleReplays(Grpc.RequestFilter request, ServerCallContext context)
    {
        //var response = await _replayProvider.CollectReplaysAsync(null);
        return await Task.FromResult(new Grpc.SimpleReplaysResponse
        {
            Replays = {  },
        });
    }
}