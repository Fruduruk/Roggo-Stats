using Discord;

using Ionic.Zip;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Discord_Bot.CustomLogging
{
    public class FileLogger : ILogger
    {
        private readonly string _name;
        private readonly FileLoggerConfiguration _config;
        private readonly List<string> _logStash;

        public FileLogger(string name, FileLoggerConfiguration config)
        {
            _name = name.Substring(name.LastIndexOf('.') + 1);
            _config = config;
            _logStash = new List<string>();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var logLine = $"{logLevel} - {formatter(state, exception)}";
            Log(logLine);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= _config.LogLevel;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        #region oldLog
        private static void ZipOldLog(string fileName)
        {
            ZipFile archive;
            if (!File.Exists(Constants.DiscordLogsZipPath))
                archive = new ZipFile(Constants.DiscordLogsZipPath);
            else
                archive = ZipFile.Read(Constants.DiscordLogsZipPath);
            archive.AddEntry($"{Path.GetFileNameWithoutExtension(fileName)} {DateTime.Now:yyyy-MM-dd hh-mm-ss}.txt", File.ReadAllText(fileName));
            archive.Save();
            File.Delete(fileName);
        }

        private void Log(string text)
        {
            text = $"{DateTime.Now}: {text}";
            var fileName = Path.Combine(Constants.RLStatsDiscordFolder.ToString(), _name) + ".txt";
            try
            {
                lock (_logStash)
                {
                    _logStash.Add(text);
                    File.AppendAllLines(fileName, _logStash);
                    _logStash.Clear();
                }
            }
            catch
            {
                lock (_logStash)
                {
                    _logStash.Add(text);
                }
            }
            try
            {
                ZipOldLogIfTooBig(fileName);
            }
            catch { }
        }

        private void ZipOldLogIfTooBig(string fileName)
        {
            bool zipIt = false;
            using (var file = File.OpenRead(fileName))
            {
                if (file.Length > 50000)
                    zipIt = true;
            }
            if (zipIt)
                ZipOldLog(fileName);
        }
        #endregion
    }
}
