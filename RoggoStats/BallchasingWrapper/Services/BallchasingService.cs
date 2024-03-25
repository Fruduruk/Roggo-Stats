using BallchasingWrapper.BusinessLogic;
using BallchasingWrapper.DB.MongoDB;
using BallchasingWrapper.Interfaces;
using BallchasingWrapper.Models;
using Grpc.Core;

namespace BallchasingWrapper.Services;

public class BallchasingService : Grpc.Ballchasing.BallchasingBase
{
    private readonly IBallchasingApi _api;
    private readonly RlStatsMongoDatabase _db;
    private readonly ILogger<BallchasingService> _logger;

    public BallchasingService(IBallchasingApi api,RlStatsMongoDatabase db,ILogger<BallchasingService> logger)
    {
        _api = api;
        _db = db;
        _logger = logger;
    }

    public override async Task<Grpc.SimpleReplaysResponse> GetSimpleReplays(Grpc.RequestFilter request,
        ServerCallContext context)
    {
        var urlCreator = new ApiUrlCreator(request);
        var collector = new ReplayCollector(urlCreator,_api,_db,_logger);
        
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
        }).ToList();

        return await Task.FromResult(new Grpc.SimpleReplaysResponse
        {
            Count = grpcReplays.Count,
            Replays = { grpcReplays }
        });
    }
}