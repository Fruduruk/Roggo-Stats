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
            DrawCharts<AveragePlayerCore>(coreWP);
            DrawCharts<AveragePlayerBoost>(boostWP);
            DrawCharts<AveragePlayerMovement>(movementWP);
            DrawCharts<AveragePlayerPositioning>(positioningWP);
            DrawCharts<AveragePlayerDemo>(demoWP);
        }
        public void DrawCharts<T>(WrapPanel wrapPanel)
        {
            wrapPanel.Children.Clear();
            foreach (PropertyInfo avgPlayerStatsProperty in typeof(T).GetProperties())
            {
                var chart = new Chart();
                chart.Title = avgPlayerStatsProperty.Name.Replace('_', ' ');
                var barValues = new Dictionary<string, double>();
                foreach (var playerStats in AvgPlayerStatList)
                {
                    bool found = false;
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
                wrapPanel.Children.Add(chart);
                chart.ReDraw();
            }
        }
    }
}
