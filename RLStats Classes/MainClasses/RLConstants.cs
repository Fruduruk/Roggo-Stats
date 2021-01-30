using System;
using System.IO;

namespace RLStats_Classes.MainClasses
{
    public class RLConstants
    {
        public const int CurrentSeason = 2;

        public static string DebugKey { get => GetDebugKey(); }

        private static string GetDebugKey()
        {
            var keyPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\rlStatsDebugKey.txt";
            if (File.Exists(keyPath))
            {
                var key = File.ReadAllText(keyPath);
                if (!string.IsNullOrEmpty(key))
                    return key;
            }
            else
            {
                File.Create(keyPath);
            }
            throw new Exception("No debug key found\n Paste a ballchasing.com authorization key into file: Documents\rlStatsDebugKey.txt");
        }
    }
}
