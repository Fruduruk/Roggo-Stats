using Newtonsoft.Json;

using System.Diagnostics;
using System.Threading.Tasks;
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
            
        }

        private void loadingCanvas_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            var canvas = sender as Canvas;
            if(canvas is not null)
            {
                
            }
        }
    }
}
