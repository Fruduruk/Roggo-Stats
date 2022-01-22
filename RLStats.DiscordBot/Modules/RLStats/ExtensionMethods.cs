using RLStats_Classes.AverageModels;
using System.Collections.Generic;

namespace Discord_Bot.Modules.RLStats
{
    public static class ExtensionMethods
    {
        public static void AddStringToNames(this IEnumerable<AveragePlayerStats> averagePlayerStats, string text)
        {
            foreach (var averageStat in averagePlayerStats)
            {
                averageStat.PlayerName += text;
            }
        }

        public static void ConvertDansSteamIdToName(this IEnumerable<AveragePlayerStats> averagePlayerStats)
        {
            foreach (var averageStat in averagePlayerStats)
            {
                if (averageStat.PlayerName.Equals("76561198095673686"))
                {
                    averageStat.PlayerName = "Danschl";
                }
            }
        }
    }
}
