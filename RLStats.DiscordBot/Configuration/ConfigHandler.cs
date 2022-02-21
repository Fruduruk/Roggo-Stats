using RLStatsClasses.CacheHandlers;

using System;
using System.Collections.Generic;
using System.IO;

namespace Discord_Bot.Configuration
{
    public class ConfigHandler<T> where T : IEquatable<T>, new()
    {
        public string ConfigFilePath { get; private set; }

        public ConfigHandler(string filePath)
        {
            ConfigFilePath = filePath;
        }

        public List<T> Config => ReadConfigFile();

        public bool HasConfigEntryInIt(T entry)
        {
            if (Config is null)
                return false;
            if (entry is null)
                return false;
            lock (Config)
            {
                foreach (var configEntry in Config)
                {
                    if (configEntry.Equals(entry))
                        return true;
                }
            }
            return false;
        }

        public void RemoveConfigEntry(T entry)
        {
            var config = ReadConfigFile();
            config.Remove(entry);
            SaveConfigFile(config);
        }

        public void AddConfigEntry(T entry)
        {
            var config = ReadConfigFile();
            config.Add(entry);
            SaveConfigFile(config);
        }

        public void SaveConfigFile(IEnumerable<T> value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            File.WriteAllBytes(ConfigFilePath, Compressor.ConvertObject(new List<T>(value), false));
        }

        private List<T> ReadConfigFile()
        {
            try
            {
                var config = Compressor.ConvertObject<List<T>>(File.ReadAllBytes(ConfigFilePath), false);
                return config;
            }
            catch
            {
                return new List<T>();
            }
        }
    }
}
