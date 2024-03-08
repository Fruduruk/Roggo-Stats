namespace BallchasingWrapper.Extensions;

public static class LoggerExtensions
{
    public static void LogDebugObject(this ILogger logger, object obj)
    {
        logger.LogDebug(JsonConvert.SerializeObject(obj, Formatting.Indented));
    }
}