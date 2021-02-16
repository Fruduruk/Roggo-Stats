using System;
using System.IO;

namespace RLStats_Classes.MainClasses
{
    public class RLConstants
    {
        public const int CurrentSeason = 2;

        public static string DebugKey => GetDebugKey();

        public static string RLStatsFolder => GetRLStatsFolder();

        private static string GetDebugKey()
        {
                var key = File.ReadAllText(GetRLStatsDebugKeyFilePath());
                if (!string.IsNullOrEmpty(key))
                    return key.Trim();
            throw new Exception($"No debug key found\n Paste a ballchasing.com authorization key into file: {GetRLStatsDebugKeyFilePath()}");
        }

        private static string GetRLStatsFolder()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            path += @"\Rocket League Stats";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            } 
            return path;
        }

        private static string GetRLStatsDebugKeyFilePath()
        {
            var keyPath = GetRLStatsFolder() + @"\rlStatsDebugKey.txt";
            if (!File.Exists(keyPath))
                File.Create(keyPath).Dispose();
            return keyPath;
        }
    }
}
