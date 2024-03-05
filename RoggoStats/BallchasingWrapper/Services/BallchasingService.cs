using BallchasingWrapper.BusinessLogic;
using BallchasingWrapper.DB.MongoDB;
using BallchasingWrapper.Models;
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

    public override async Task<Grpc.SimpleReplaysResponse> GetSimpleReplays(Grpc.RequestFilter request,
        ServerCallContext context)
    {
        var response =
            await _replayProvider.CollectReplaysAsync(
                new APIRequestFilter { CheckName = true, Names = { "Fruduruk" } }, true);


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