using Grpc.Core;
using BallchasingWrapper;

namespace BallchasingWrapper.Services;

public class GreeterService : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger;

    public GreeterService(ILogger<GreeterService> logger)
    {
        _logger = logger;
    }

    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        return Task.FromResult(new HelloReply
        {
            Message = "Hello " + request.Name + " with the age of " + request.Age,
            Data = new Data
            {
                DataPoint1 = request.Name,
                DataPoint2 = request.Age
            }
        });
    }
}