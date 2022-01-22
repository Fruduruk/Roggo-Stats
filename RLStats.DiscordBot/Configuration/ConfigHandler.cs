using Discord_Bot.Exceptions;

using RLStats_Classes.MainClasses;

using System;
using System.IO;

namespace Discord_Bot.Configuration
{
    public static class ConfigHandler
    {
        public static DirectoryInfo RLStatsDiscordFolder => GetRLStatsDiscordFolder();
        private static string ConfigFilePath => Path.Combine(RLStatsDiscordFolder.ToString(), "DiscordConfig.json.7z");

        private static DirectoryInfo GetRLStatsDiscordFolder()
        {
            var folderPath = Path.Combine(RLConstants.RLStatsFolder, "Discord");
            return Directory.CreateDirectory(folderPath);
        }

        public static Config Config
        {
            get => ReadConfigFile();
            set => SaveConfigFile(value);
        }

        public static bool HasConfigEntryInIt(this Config config, ConfigEntry entry)
        {

            if (config is null)
                return false;
            if (entry is null)
                return false;
            lock (Config)
            {
                foreach (var configEntry in config.ConfigEntries)
                {
                    if (configEntry.Equals(entry))
                        return true;
                }
            }
            return false;
        }

        public static void RemoveConfigEntry(ConfigEntry entry)
        {
            var newConfig = Config;
            newConfig.ConfigEntries.Remove(entry);
            Config = newConfig;
        }

        private static void SaveConfigFile(Config value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            File.WriteAllBytes(ConfigFilePath, Compressor.ConvertObject(value, false));
        }

        public static ConfigEntry UpdateLastPost(ConfigEntry entry, DateTime newLastPost)
        {
            ConfigEntry newEntry = null;
            var config = Config;
            foreach (var configEntry in config.ConfigEntries)
            {
                if (configEntry.Equals(entry))
                {
                    configEntry.LastPost = newLastPost;
                    newEntry = configEntry;
                }
            }
            Config = config;
            if (newEntry is null)
                throw new EntryNotFoundException();
            return newEntry;
        }

        private static Config ReadConfigFile()
        {
            try
            {
                var config = Compressor.ConvertObject<Config>(File.ReadAllBytes(ConfigFilePath), false);
                return config;
            }
            catch
            {
                return new Config();
            }
        }
    }
}
