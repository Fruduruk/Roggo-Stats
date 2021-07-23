using Newtonsoft.Json;

using RLStats_Classes.Models;

using System.Windows.Controls;

namespace RocketLeagueStats.Components
{
    /// <summary>
    /// Interaction logic for LoadingGrid.xaml
    /// </summary>
    public partial class LoadingGrid : UserControl
    {
        public LoadingGrid()
        {
            InitializeComponent();
        }

        public void UpdateView(ProgressState e)
        {
            Dispatcher.Invoke(() =>
            {
                MainTextBlock.Text = JsonConvert.SerializeObject(e, Formatting.Indented);
            });
        }
    }
}
