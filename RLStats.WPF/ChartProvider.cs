using RLStats_Classes.Models.Average;

using System;
using System.Collections.Generic;
using System.Reflection;

namespace RLStats_WPF
{
    public class ChartProvider
    {
        public IEnumerable<AveragePlayerStats> AvgPlayerStatList { get; }
        public IEnumerable<AveragePlayerStats> AvgPlayerStatListToCompare { get; }

        public ChartProvider(IEnumerable<AveragePlayerStats> statList)
        {
            AvgPlayerStatList = statList;
        }

        public ChartProvider(IEnumerable<AveragePlayerStats> statList, IEnumerable<AveragePlayerStats> statListToCompare)
        {
            AvgPlayerStatList = statList;
            AvgPlayerStatListToCompare = statListToCompare;
        }

        public IEnumerable<ChartCreator> GetChartCreators<T>(double height = (375 + 100), double width = 700)
        {
            var chartList = new List<ChartCreator>();
            if (AvgPlayerStatListToCompare is null)
            {
                foreach (var avgPlayerStatsProperty in typeof(T).GetProperties())
                {
                    var barValues = GetChartBarValuesForStatProperty(AvgPlayerStatList, avgPlayerStatsProperty);
                    var title = avgPlayerStatsProperty.Name.Replace('_', ' ');
                    var chart = new ChartCreator(title, barValues, height, width);
                    chartList.Add(chart);
                }
            }
            else
            {
                foreach (var avgPlayerStatsProperty in typeof(T).GetProperties())
                {
                    var firstBarValues = GetChartBarValuesForStatProperty(AvgPlayerStatList, avgPlayerStatsProperty);
                    var secondBarValues = GetChartBarValuesForStatProperty(AvgPlayerStatListToCompare, avgPlayerStatsProperty);
                    var title = avgPlayerStatsProperty.Name.Replace('_', ' ');
                    var chart = new ChartCreator(title, firstBarValues, secondBarValues, height, width);
                    chartList.Add(chart);
                }
            }

            return chartList;
        }

        private Dictionary<string, double> GetChartBarValuesForStatProperty(
            IEnumerable<AveragePlayerStats> averagePlayerStats, PropertyInfo avgPlayerStatsProperty)
        {
            var barValues = new Dictionary<string, double>();
            foreach (var playerStats in averagePlayerStats)
            {
                var found = false;
                foreach (var avgsp in playerStats.GetType().GetProperties())
                {
                    object svgStatsPropertyValue = avgsp.GetValue(playerStats);
                    foreach (var avgCP in svgStatsPropertyValue.GetType().GetProperties())
                    {
                        if (avgCP.Name.Equals(avgPlayerStatsProperty.Name))
                        {
                            barValues.Add(playerStats.PlayerName, (double)avgCP.GetValue(svgStatsPropertyValue));
                            found = true;
                            break;
                        }
                    }

                    if (found)
                        break;
                }
            }

            return barValues;
        }

        /// <summary>
        /// This returns a set of chart creators for specific charts.
        /// </summary>
        /// <param name="propertyNames"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public IEnumerable<ChartCreator> GetSpecificCreators(IEnumerable<string> propertyNames, double height = (375 + 100), double width = 700)
        {
            var allPropertyNames = new List<string>(AveragePlayerStats.GetAllPropertyNames());
            foreach (var propertyName in propertyNames)
            {
                if (!allPropertyNames.Contains(propertyName))
                    throw new ArgumentOutOfRangeException(propertyName);
            }

            var chartList = new List<ChartCreator>();

            chartList.AddRange(GetSpecificCreators<AveragePlayerCore>(propertyNames, height, width));
            chartList.AddRange(GetSpecificCreators<AveragePlayerBoost>(propertyNames, height, width));
            chartList.AddRange(GetSpecificCreators<AveragePlayerMovement>(propertyNames, height, width));
            chartList.AddRange(GetSpecificCreators<AveragePlayerPositioning>(propertyNames, height, width));
            chartList.AddRange(GetSpecificCreators<AveragePlayerDemo>(propertyNames, height, width));

            return chartList;
        }

        public IEnumerable<ChartCreator> GetSpecificCreators<T>(IEnumerable<string> propertyNames, double height = (375 + 100), double width = 700)
        {
            var chartList = new List<ChartCreator>();
            var properties = typeof(T).GetProperties();
            var propertyNameList = new List<string>(propertyNames);

            foreach (var property in properties)
            {
                if (propertyNameList.Contains(property.Name))
                {
                    var barValues = GetChartBarValuesForStatProperty(AvgPlayerStatList, property);
                    var title = property.Name.Replace('_', ' ');
                    if (AvgPlayerStatListToCompare is null)
                    {
                        var chart = new ChartCreator(title, barValues, height, width);
                        chartList.Add(chart);
                    }
                    else
                    {
                        var secondBarValues = GetChartBarValuesForStatProperty(AvgPlayerStatListToCompare, property);
                        var chart = new ChartCreator(title, barValues, secondBarValues, height, width);
                        chartList.Add(chart);
                    }
                }
            }

            return chartList;
        }
    }
}
