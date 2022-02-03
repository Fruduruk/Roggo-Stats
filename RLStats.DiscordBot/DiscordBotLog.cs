using Ionic.Zip;

using System;
using System.IO;

namespace Discord_Bot
{
    public static class DiscordBotLog
    {
        public static void LogCalls(string[] calls)
        {
            File.AppendAllLines(Constants.LogPath, calls);
        }

        private static void ZipOldLog()
        {
            ZipFile archive;
            if (!File.Exists(Constants.DiscordLogsZipPath))
                archive = new ZipFile(Constants.DiscordLogsZipPath);
            else
                archive = ZipFile.Read(Constants.DiscordLogsZipPath);
            archive.AddEntry($"{DateTime.Now:yyyy-MM-dd hh-mm-ss}.txt", File.ReadAllText(Constants.LogPath));
            archive.Save();
            File.Delete(Constants.LogPath);
        }

        public static void Log(string text)
        {
            text = $"{DateTime.Now}: {text}";

            File.AppendAllLines(Constants.LogPath, new string[] { text });
            ZipOldLogIfTooBig();
        }

        private static void ZipOldLogIfTooBig()
        {
            bool zipIt = false;
            using (var file = File.OpenRead(Constants.LogPath))
            {
                if (file.Length > 5000)
                    zipIt = true;
            }
            if (zipIt)
                ZipOldLog();
        }
    }
}
