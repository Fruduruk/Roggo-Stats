using RLStats_Classes.AverageModels;
using System.Collections.Generic;
using System.Reflection;

namespace RLStats_WPF
{
    public class ChartProvider
    {
        public IEnumerable<AveragePlayerStats> AvgPlayerStatList { get; }

        public ChartProvider(IEnumerable<AveragePlayerStats> statList)
        {
            AvgPlayerStatList = statList;
        }

        public IEnumerable<ChartCreator> GetChartCreators<T>(double height = (375 + 100), double width = 700)
        {
            var chartList = new List<ChartCreator>();

            foreach (var avgPlayerStatsProperty in typeof(T).GetProperties())
            {
                var barValues = GetChartBarValuesForStatProperty(avgPlayerStatsProperty);
                var title = avgPlayerStatsProperty.Name.Replace('_', ' ');
                var chart = new ChartCreator(title, barValues, height, width);
                chartList.Add(chart);
            }

            return chartList;
        }



        private Dictionary<string, double> GetChartBarValuesForStatProperty(PropertyInfo avgPlayerStatsProperty)
        {
            var barValues = new Dictionary<string, double>();
            foreach (var playerStats in AvgPlayerStatList)
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
    }
}
