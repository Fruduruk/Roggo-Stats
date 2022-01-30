using RLStats_Classes.MainClasses;

using System.IO;

namespace Discord_Bot
{
    public class Constants
    {
        /// <summary>
        /// Proceeding method remark
        /// </summary>
        public const string IgnoreEndpoint = "093wmasoiwej,,na9w3..pnfc_iosdnpvaiouebg";
        public static DirectoryInfo RLStatsDiscordFolder => GetRLStatsDiscordFolder();
        public static string SubscribtionConfigFilePath => GetSubscrptionConfigFilePath();
        public static string UserFavoritesConfigFilePath => GetUserFavoritesConfigFilePath();

        private static string GetSubscrptionConfigFilePath()
        {
            return Path.Combine(RLStatsDiscordFolder.ToString(), "SubscriptionConfig.json.7z");
        }

        private static string GetUserFavoritesConfigFilePath()
        {
            return Path.Combine(RLStatsDiscordFolder.ToString(), "UserFavoritesConfig.json.7z");
        }

        private static DirectoryInfo GetRLStatsDiscordFolder()
        {
            var folderPath = Path.Combine(RLConstants.RLStatsFolder, "Discord");
            return Directory.CreateDirectory(folderPath);
        }
    }
}
