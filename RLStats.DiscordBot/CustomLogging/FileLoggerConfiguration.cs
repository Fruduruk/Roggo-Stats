using Microsoft.Extensions.Logging;

namespace Discord_Bot.CustomLogging
{
    public class FileLoggerConfiguration
    {
        public LogLevel LogLevel { get; set; }

        public FileLoggerConfiguration(LogLevel logLevel)
        {
            LogLevel = logLevel;
        }
    }
}