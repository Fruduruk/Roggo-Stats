using Ionic.Zip;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.CustomLogging
{
    public static class FileWriter
    {
        private static readonly List<string> _logStash = new List<string>();
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

        public static void Write(string text)
        {
            text = $"{DateTime.Now}: {text}";
            var fileName = Constants.LogPath;
            lock (_logStash)
            {
                try
                {
                    _logStash.Add(text);
                    File.AppendAllLines(fileName, _logStash);
                    _logStash.Clear();
                }
                catch
                {
                    _logStash.Add(text);
                }
                try
                {
                    ZipOldLogIfTooBig(fileName);
                }
                catch { }
            }

        }

        private static void ZipOldLogIfTooBig(string fileName)
        {
            bool zipIt = false;
            using (var file = File.OpenRead(fileName))
            {
                if (file.Length > 1024 * 1024 * 4)
                    zipIt = true;
            }
            if (zipIt)
                ZipOldLog(fileName);
        }
    }
}
