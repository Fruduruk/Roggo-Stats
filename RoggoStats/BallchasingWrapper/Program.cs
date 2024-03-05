global using Newtonsoft.Json;
using BallchasingWrapper.Services;

const string ballchasingApiKey = "BALLCHASING_API_KEY";

Console.WriteLine("Hi, this is the Ballchasing Wrapper for Roggo Stats!");
Console.WriteLine("It is the main Interface that controls the access to the Ballchasing API.");
Console.WriteLine("Your access to the data ist via gRPC.");

if (!ApiKeyAvailable())
{
    Console.WriteLine("For this service to work you need to give it a Ballchasing API-Key. This key controls the call limit and can be generated here: https://ballchasing.com/upload");
    Console.WriteLine($"To set the API-Key you need to set the environment variable '{ballchasingApiKey}'.");
    return;
}

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<BallchasingService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
return;


bool ApiKeyAvailable()
{
    var key = Environment.GetEnvironmentVariable(ballchasingApiKey);
    return key is not null;
}