using Discord_Bot.Exceptions;

using RLStats_Classes.MainClasses;

using System;
using System.IO;

namespace Discord_Bot.Configuration
{
    public class ConfigHandler<ConfigType, EntryType> where ConfigType : IConfigList<EntryType>, new() where EntryType : IEquatable<EntryType>, new()
    {
        public string ConfigFilePath { get; private set; } 

        public ConfigHandler(string filePath)
        {
            ConfigFilePath = filePath;
        }

        public ConfigType Config => ReadConfigFile();

        public bool HasConfigEntryInIt(EntryType entry)
        {
            if (Config is null)
                return false;
            if (entry is null)
                return false;
            lock (Config)
            {
                foreach (var configEntry in Config.ConfigEntries)
                {
                    if (configEntry.Equals(entry))
                        return true;
                }
            }
            return false;
        }

        public void RemoveConfigEntry(EntryType entry)
        {
            var config = ReadConfigFile();
            config.ConfigEntries.Remove(entry);
            SaveConfigFile(config);
        }

        public void AddConfigEntry(EntryType entry)
        {
            var config = ReadConfigFile();
            config.ConfigEntries.Add(entry);
            SaveConfigFile(config);
        }

        public void SaveConfigFile(ConfigType value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            File.WriteAllBytes(ConfigFilePath, Compressor.ConvertObject(value, false));
        }

        private ConfigType ReadConfigFile()
        {
            try
            {
                var config = Compressor.ConvertObject<ConfigType>(File.ReadAllBytes(ConfigFilePath), false);
                return config;
            }
            catch
            {
                return new ConfigType();
            }
        }
    }
}
