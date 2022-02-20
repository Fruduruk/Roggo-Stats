using RLStatsClasses.Models.ReplayModels.Average;

using RLStatsWPF;

using System.Collections.Generic;
using System.Windows.Controls;

namespace RocketLeagueStats.Components
{
    public partial class ChartDisplay : UserControl
    {
        public IEnumerable<AveragePlayerStats> AvgPlayerStatList { get; set; } = new List<AveragePlayerStats>();
        public IEnumerable<AveragePlayerStats> AvgPlayerStatListToCompare { get; set; }
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
            ChartProvider chartProvider;
            if (AvgPlayerStatListToCompare is null)
                chartProvider = new ChartProvider(AvgPlayerStatList);
            else
                chartProvider = new ChartProvider(AvgPlayerStatList, AvgPlayerStatListToCompare);

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
