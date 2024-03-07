using BallchasingWrapper.BusinessLogic;
using BallchasingWrapper.DB.MongoDB;
using BallchasingWrapper.Interfaces;
using BallchasingWrapper.Models;
using Grpc.Core;

namespace BallchasingWrapper.Services;

public class BallchasingService : Grpc.Ballchasing.BallchasingBase
{
    private readonly ILogger<BallchasingService> _logger;
    private readonly ReplayCollector _replayCollector;
    //private readonly AdvancedReplayProvider _advancedReplayProvider;

    public BallchasingService(ILogger<BallchasingService> logger, IBallchasingApi api, RlStatsMongoDatabase db)
    {
        _logger = logger;
        _replayCollector = new ReplayCollector(api, db, logger);
        //_advancedReplayProvider = new AdvancedReplayProvider(api, db, logger);
    }

    public override async Task<Grpc.SimpleReplaysResponse> GetSimpleReplays(Grpc.RequestFilter request,
        ServerCallContext context)
    {
        var filter = new ApiUrlCreator(request);
        
        var response =
            await _replayCollector.CollectReplaysAsync(filter, CancellationToken.None);
        

        var replays = response.Replays.ToList();
        var grpcReplays = new List<Grpc.Replay>();
        for (int i = 0; i< response.Replays.Count(); i++)
        {
            var replay = replays[i];
            try
            {
                var grpcReplay = replay.ToGrpcReplay();
                grpcReplays.Add(grpcReplay);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }
        //var grpcReplays = response.Replays.Select(r => r.ToGrpcReplay()).ToList();

        return await Task.FromResult(new Grpc.SimpleReplaysResponse
        {
            Replays = { grpcReplays }
        });
    }
}