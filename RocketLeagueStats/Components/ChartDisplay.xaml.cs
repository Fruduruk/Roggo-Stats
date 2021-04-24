using RLStats_Classes.AverageModels;
using RLStats_Classes.MainClasses;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Controls;
using RLStats_WPF;

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
            var chartProvider = new ChartProvider(AvgPlayerStatList);

            wrapPanel.Children.Clear();
            var chartsCreators = chartProvider.GetChartCreators<T>();
            foreach (var chartCreator in chartsCreators)
            {
                var chart = new Chart(chartCreator);
                chart.ReDraw();
                wrapPanel.Children.Add(chart);
            }
        }
    }
}
