
using Microsoft.Extensions.Logging;

using System;

namespace Discord_Bot.CustomLogging
{
    public class FileLogger : ILogger
    {
        private readonly string _name;
        private readonly FileLoggerConfiguration _config;

        public FileLogger(string name, FileLoggerConfiguration config)
        {
            _name = name.Substring(name.LastIndexOf('.') + 1);
            _config = config;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var logLine = $"{_name} - {logLevel} - {formatter(state, exception)}";
            FileWriter.Write(logLine);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= _config.LogLevel;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

    }
}
