using RLStats_Classes.Models.Advanced;

using System;
using System.Collections.Generic;

namespace RLStats_Classes.Models.Average
{
    public class AveragePlayerStats
    {
        public string PlayerName { get; set; }
        public AveragePlayerCore AverageCore { get; set; }
        public AveragePlayerBoost AverageBoost { get; set; }
        public AveragePlayerMovement AverageMovement { get; set; }
        public AveragePlayerPositioning AveragePositioning { get; set; }
        public AveragePlayerDemo AverageDemo { get; set; }
        public bool IsEmpty => IsEverythingEmpty();

        private bool IsEverythingEmpty()
        {
            return IsPartEmpty(AverageCore)
                && IsPartEmpty(AverageBoost)
                && IsPartEmpty(AverageMovement)
                && IsPartEmpty(AveragePositioning)
                && IsPartEmpty(AverageDemo);
        }

        private static bool IsPartEmpty<T>(T avg) where T : new()
        {
            var properties = typeof(T).GetProperties();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(avg) as double?;
                if (value is not null)
                    if (value.HasValue)
                        if (value.Value != 0)
                            return false;
            }
            return true;
        }


        public static TAvgt GetAverage<TAvgt, T>(List<PlayerStats> allStatsForOnePlayer) where TAvgt : new() where T : new()
        {
            TAvgt avgt = new TAvgt();
            var properties = typeof(T).GetProperties();
            foreach (var p in properties)
            {
                var values = new List<double>();
                foreach (var stats in allStatsForOnePlayer)
                {
                    var playerStatsProperties = typeof(PlayerStats).GetProperties();
                    foreach (var psp in playerStatsProperties)
                    {
                        if (typeof(T).Name.Equals(psp.Name))
                        {
                            var propertyValue = psp.GetValue(stats);
                            var v = p.GetValue(propertyValue);
                            switch (v)
                            {
                                case double d:
                                    AddIfItHasValue(values, d);
                                    break;
                                case bool b:
                                    AddIfItHasValue(values, b);
                                    break;
                                case int i:
                                    AddIfItHasValue(values, i);
                                    break;
                                case float f:
                                    AddIfItHasValue(values, f);
                                    break;
                            }
                        }
                    }
                }
                var avgProperties = typeof(TAvgt).GetProperties();
                foreach (var avgP in avgProperties)
                    if (avgP.Name.Equals(p.Name))
                    {
                        avgP.SetValue(avgt, GetAverage(values));
                        break;
                    }
            }
            return avgt;
        }
        public static AveragePlayerStats GetAveragePlayerStats(List<PlayerStats> allStatsForOnePlayer, string playerName)
        {
            var average = new AveragePlayerStats
            {
                PlayerName = playerName,
                AverageCore = GetAverage<AveragePlayerCore, PlayerCore>(allStatsForOnePlayer),
                AverageBoost = GetAverage<AveragePlayerBoost, PlayerBoost>(allStatsForOnePlayer),
                AverageMovement = GetAverage<AveragePlayerMovement, PlayerMovement>(allStatsForOnePlayer),
                AveragePositioning = GetAverage<AveragePlayerPositioning, PlayerPositioning>(allStatsForOnePlayer),
                AverageDemo = GetAverage<AveragePlayerDemo, Demo>(allStatsForOnePlayer)
            };
            return average;
        }
        private static void AddIfItHasValue(List<double> list, double? value)
        {
            if (value.HasValue)
                list.Add(Convert.ToDouble(value));
        }
        private static void AddIfItHasValue(List<double> list, int? value)
        {
            if (value.HasValue)
                list.Add(Convert.ToDouble(value));
        }
        private static void AddIfItHasValue(List<double> list, float? value)
        {
            if (value.HasValue)
                list.Add(Convert.ToDouble(value));
        }
        private static void AddIfItHasValue(List<double> list, bool? value)
        {
            if (value.HasValue)
                list.Add(value == true ? 1 : 0);
        }
        private static double GetAverage(List<double> list)
        {
            double average = 0;
            foreach (var value in list)
                average += value / list.Count;
            return average;
        }

        public static string[] GetAllPropertyNames()
        {
            var names = new List<string>();
            names.AddRange(GetPropertyNames<AveragePlayerCore>());
            names.AddRange(GetPropertyNames<AveragePlayerBoost>());
            names.AddRange(GetPropertyNames<AveragePlayerMovement>());
            names.AddRange(GetPropertyNames<AveragePlayerPositioning>());
            names.AddRange(GetPropertyNames<AveragePlayerDemo>());
            return names.ToArray();
        }

        private static IEnumerable<string> GetPropertyNames<T>()
        {
            var names = new List<string>();
            var properties = typeof(T).GetProperties();
            foreach (var property in properties)
            {
                names.Add(property.Name);
            }
            return names;
        }
    }
}