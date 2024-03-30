using BallchasingWrapper.BusinessLogic;
using BallchasingWrapper.Interfaces;
using BallchasingWrapper.Models;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace BallchasingWrapper.Services;

public class BallchasingService : Grpc.Ballchasing.BallchasingBase
{
    private readonly IBallchasingApi _api;
    private readonly IReplayCache _replayCache;
    private readonly IDatabase _database;
    private readonly BackgroundDownloadingService _backgroundDownloadingService;
    private readonly ILogger<BallchasingService> _logger;

    public BallchasingService(IBallchasingApi api, IReplayCache replayCache, IDatabase database,
        BackgroundDownloadingService backgroundDownloadingService,
        ILogger<BallchasingService> logger)
    {
        _api = api;
        _replayCache = replayCache;
        _database = database;
        _backgroundDownloadingService = backgroundDownloadingService;
        _logger = logger;
    }

    public override async Task<Grpc.SimpleReplaysResponse> GetSimpleReplays(Grpc.FilterRequest request,
        ServerCallContext context)
    {
        var urlCreator = new ApiUrlCreator(request);
        var collector = new ReplayCollector(urlCreator, _api, _replayCache, _logger);

        var response =
            await collector.CollectReplaysAsync(_logger, context.CancellationToken);

        if (context.CancellationToken.IsCancellationRequested)
            return new Grpc.SimpleReplaysResponse();

        var grpcReplays = response.Replays.Select(replay => replay.ToGrpcReplay()).ToList();

        return new Grpc.SimpleReplaysResponse
        {
            Count = grpcReplays.Count,
            Replays = { grpcReplays }
        };
    }

    public override async Task<Grpc.AdvancedReplay> GetAdvancedReplayById(Grpc.IdRequest request,
        ServerCallContext context)
    {
        var downloader = new AdvancedReplayDownloader(_api, _database, _logger);
        var replay = await downloader.LoadAdvancedReplayByIdAsync(request.Id, context.CancellationToken);
        if (context.CancellationToken.IsCancellationRequested || replay is null)
            return new Grpc.AdvancedReplay();

        return replay.ToGrpcAdvancedReplay();
    }

    public override async Task<Grpc.AdvancedReplaysResponse> GetAdvancedReplays(Grpc.FilterRequest request,
        ServerCallContext context)
    {
        var simpleReplays = await GetSimpleReplays(request, context);
        var downloader = new AdvancedReplayDownloader(_api, _database, _logger);
        var advancedReplays =
            (await downloader.LoadAdvancedReplaysByIdsAsync(simpleReplays.Replays.Select(replay => replay.Id),
                context.CancellationToken)).ToList();
        if (context.CancellationToken.IsCancellationRequested)
            return new Grpc.AdvancedReplaysResponse { Replays = { Array.Empty<Grpc.AdvancedReplay>() } };

        return new Grpc.AdvancedReplaysResponse
        {
            Count = advancedReplays.Count,
            Replays = { advancedReplays.Select(advancedReplay => advancedReplay.ToGrpcAdvancedReplay()) }
        };
    }

    public override async Task<Grpc.BackgroundDownloadResponse> GetBackgroundDownloadOperations(Empty request,
        ServerCallContext context)
    {
        return new Grpc.BackgroundDownloadResponse
        {
            Operations = { await _backgroundDownloadingService.GetBackgroundDownloadOperations() }
        };
    }

    public override async Task<Grpc.BasicResponse> StartBackgroundDownload(Grpc.BackgroundDownloadRequest request,
        ServerCallContext context)
    {
        var success = await
            _backgroundDownloadingService.InsertNewBackgroundDownloadAsync(request.Operation);
        return new Grpc.BasicResponse
        {
            Success = success,
            Error = success ? string.Empty : "Downloader already exists."
        };
    }

    public override async Task<Grpc.BasicResponse> CancelBackgroundDownload(Grpc.Identity request, ServerCallContext context)
    {
        var success = await
            _backgroundDownloadingService.CancelBackgroundDownloadAsync(request);
        return new Grpc.BasicResponse
        {
            Success = success,
            Error = success ? string.Empty : "Downloader does not exist."
        };
    }
}