using RLStats_Classes.MainClasses;

using System.IO;

namespace Discord_Bot
{
    public class Constants
    {
        public static DirectoryInfo RLStatsDiscordFolder => GetRLStatsDiscordFolder();
        public static string SubscribtionConfigFilePath => GetSubscrptionConfigFilePath();

        private static string GetSubscrptionConfigFilePath()
        {
            return Path.Combine(RLStatsDiscordFolder.ToString(), "DiscordConfig.json.7z");
        }

        private static DirectoryInfo GetRLStatsDiscordFolder()
        {
            var folderPath = Path.Combine(RLConstants.RLStatsFolder, "Discord");
            return Directory.CreateDirectory(folderPath);
        }


    }
}
