using BallchasingWrapper.Models.ReplayModels;

namespace BallchasingWrapper;

public static class GrpcConvert
{
    public static Grpc.Replay ToGrpcReplay(this Replay replay)
    {
        return new Grpc.Replay();
    }
}