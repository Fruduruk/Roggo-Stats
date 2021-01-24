using RLStats_Classes.AverageModels;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Controls;

namespace RocketLeagueStats.Components
{
    public partial class ChartDisplay : UserControl
    {
        public Dictionary<string, AveragePlayerStats> AvgPlayerStatList { get; set; } = new Dictionary<string, AveragePlayerStats>();
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
                Chart chart = new Chart();
                chart.Title = avgPlayerStatsProperty.Name.Replace('_', ' ');
                Dictionary<string, double> barValues = new Dictionary<string, double>();
                foreach (KeyValuePair<string, AveragePlayerStats> pair in AvgPlayerStatList)
                {
                    bool found = false;
                    AveragePlayerStats avgStats = pair.Value;
                    foreach (PropertyInfo avgsp in avgStats.GetType().GetProperties())
                    {
                        object svgStatsPropertyValue = avgsp.GetValue(avgStats);
                        foreach (PropertyInfo avgCP in svgStatsPropertyValue.GetType().GetProperties())
                        {
                            if (avgCP.Name.Equals(avgPlayerStatsProperty.Name))
                            {
                                barValues.Add(pair.Key, (double)avgCP.GetValue(svgStatsPropertyValue));
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
