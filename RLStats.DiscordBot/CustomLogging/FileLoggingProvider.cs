using Microsoft.Extensions.Logging;

namespace Discord_Bot.CustomLogging
{
    public class FileLoggingProvider : ILoggerProvider
    {
        readonly FileLoggerConfiguration _configToUse;
        public FileLoggingProvider(FileLoggerConfiguration configToUse)
        {
            _configToUse = configToUse;
        }
        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(categoryName, _configToUse);
        }

        public void Dispose()
        {
            
        }
    }
}