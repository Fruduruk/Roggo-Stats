using BallchasingWrapper.BusinessLogic;
using BallchasingWrapper.DB.MongoDB;
using BallchasingWrapper.Interfaces;
using BallchasingWrapper.Models;
using Grpc.Core;

namespace BallchasingWrapper.Services;

public class BallchasingService : Grpc.Ballchasing.BallchasingBase
{
    private readonly ILogger<BallchasingService> _logger;
    private readonly ReplayCollectorFactory _replayCollectorFactory;

    public BallchasingService(ILogger<BallchasingService> logger, ReplayCollectorFactory replayCollectorFactory)
    {
        _logger = logger;
        _replayCollectorFactory = replayCollectorFactory;
    }

    public override async Task<Grpc.SimpleReplaysResponse> GetSimpleReplays(Grpc.RequestFilter request,
        ServerCallContext context)
    {
        _replayCollectorFactory.AddCurrentLogger(_logger);
        var urlCreator = new ApiUrlCreator(request);
        var collector = _replayCollectorFactory.GetReplayCollector(urlCreator);
        
        var response =
            await collector.CollectReplaysAsync(_logger, context.CancellationToken);

        var grpcReplays = response.Replays.Select(replay =>
        {
            try
            {
                return replay.ToGrpcReplay();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        });

        return await Task.FromResult(new Grpc.SimpleReplaysResponse
        {
            Replays = { grpcReplays }
        });
    }
}