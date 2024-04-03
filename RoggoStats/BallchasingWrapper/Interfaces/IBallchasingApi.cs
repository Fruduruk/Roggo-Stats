using BallchasingWrapper.Models;

namespace BallchasingWrapper.Interfaces;

public interface IBallchasingApi
{
    Task<HttpResponseMessage> GetAsync(string url, CancellationToken cancellationToken);
    Task<ApiDataPack> GetApiDataPackAsync(string url, CancellationToken cancellationToken);
    IEnumerable<string> GetAndDeleteCalls();
}