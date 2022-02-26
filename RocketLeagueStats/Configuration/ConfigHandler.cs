using RLStatsClasses;
using RLStatsClasses.CacheHandlers;

using System.IO;

namespace RocketLeagueStats.Configuration
{
    public class ConfigHandler
    {
        private readonly string fileName;

        public ConfigHandler()
        {
            fileName = Path.Combine(RLConstants.RLStatsFolder, "config");
        }
        public Config? LoadConfig()
        {
            if (!File.Exists(fileName))
                return null;
            try
            {
                var config = Compressor.ConvertObject<Config>(File.ReadAllBytes(fileName));
                return config;
            }
            catch
            {
                File.Delete(fileName);
                return null;
            }
        }

        public void SaveConfig(Config config)
        {
            var bytes = Compressor.ConvertObject(config);
            File.WriteAllBytes(fileName, bytes);
        }
    }
}
