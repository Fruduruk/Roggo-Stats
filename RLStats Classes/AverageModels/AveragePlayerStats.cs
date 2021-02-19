using RLStats_Classes.AdvancedModels;
using System;
using System.Collections.Generic;

namespace RLStats_Classes.AverageModels
{
    public class AveragePlayerStats
    {
        public AveragePlayerCore AverageCore { get; set; }
        public AveragePlayerBoost AverageBoost { get; set; }
        public AveragePlayerMovement AverageMovement { get; set; }
        public AveragePlayerPositioning AveragePositioning { get; set; }
        public AveragePlayerDemo AverageDemo { get; set; }

        public static TAvgt GetAverage<TAvgt,T>(List<PlayerStats> allStatsForOnePlayer) where TAvgt : new() where T : new()
        {
            TAvgt avgt = new TAvgt();
            var properties = typeof(T).GetProperties();
            foreach (var p in properties)
            {
                var values = new List<double>();
                foreach (var stats in allStatsForOnePlayer)
                {
                    var playerStatsProperties = typeof(PlayerStats).GetProperties();
                    foreach(var psp in playerStatsProperties)
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
        public static AveragePlayerStats GetAveragePlayerStats(List<PlayerStats> allStatsForOnePlayer)
        {
            var average = new AveragePlayerStats
            {
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

    }
}