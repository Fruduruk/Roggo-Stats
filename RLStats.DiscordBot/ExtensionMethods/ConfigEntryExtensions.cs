﻿using Discord_Bot.Configuration;

using RLStatsClasses.Models.ReplayModels.Average;

using System.Collections.Generic;

namespace Discord_Bot.ExtensionMethods
{
    public static class ConfigEntryExtensions
    {
        public static void AddPropertyNamesToConfigEntry(this Subscription configEntry, IEnumerable<int> indexNumberList, bool collectAll = false)
        {
            var dic = GetPropertyNameDictionary();
            if (collectAll)
            {
                configEntry.StatNames.AddRange(AveragePlayerStats.GetAllPropertyNames());
            }
            else
            {
                foreach (var indexNumber in indexNumberList)
                {
                    try
                    {
                        var name = dic[indexNumber];
                        configEntry.StatNames.Add(name);
                    }
                    catch
                    {
                        continue;
                    }

                }
            }
        }

        public static void AddPropertyNamesToConfigEntry(this UserFavorite userFavorite, IEnumerable<int> indexNumberList, bool collectAll = false)
        {
            var dic = GetPropertyNameDictionary();
            if (collectAll)
            {
                userFavorite.FavoriteStats.AddRange(AveragePlayerStats.GetAllPropertyNames());
            }
            else
            {
                foreach (var indexNumber in indexNumberList)
                {
                    try
                    {
                        var name = dic[indexNumber];
                        userFavorite.FavoriteStats.Add(name);
                    }
                    catch
                    {
                        continue;
                    }

                }
            }
        }

        private static Dictionary<int, string> GetPropertyNameDictionary()
        {
            var properties = AveragePlayerStats.GetAllPropertyNames();
            var dic = new Dictionary<int, string>();
            for (int i = 0; i < properties.Length; i++)
                dic.Add(i, properties[i]);
            return dic;
        }
    }
}
