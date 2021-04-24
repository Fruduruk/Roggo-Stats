using RLStats_Classes.AverageModels;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Controls;

namespace RocketLeagueStats.Components
{
    public partial class ChartDisplay : UserControl
    {
        public IEnumerable<AveragePlayerStats> AvgPlayerStatList { get; set; } = new List<AveragePlayerStats>();
        public ChartDisplay()
        {
            InitializeComponent();
        }
        public void Refresh()
        {
            ReDrawCharts<AveragePlayerCore>(coreWP);
            ReDrawCharts<AveragePlayerBoost>(boostWP);
            ReDrawCharts<AveragePlayerMovement>(movementWP);
            ReDrawCharts<AveragePlayerPositioning>(positioningWP);
            ReDrawCharts<AveragePlayerDemo>(demoWP);
        }

        private void ReDrawCharts<T>(Panel wrapPanel)
        {
            wrapPanel.Children.Clear();
            var charts = GetCharts<T>();
            foreach (var chart in charts)
            {
                wrapPanel.Children.Add(chart);
            }
        }

        private IEnumerable<Chart> GetCharts<T>()
        {
            var chartList = new List<Chart>();
            
            foreach (var avgPlayerStatsProperty in typeof(T).GetProperties())
            {
                var chart = new Chart();
                chart.Title = avgPlayerStatsProperty.Name.Replace('_', ' ');
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
                chart.ChartBarValues = barValues;
                chartList.Add(chart);
                chart.ReDraw();
            }

            return chartList;
        }
    }
}
