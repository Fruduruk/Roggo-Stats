using System.ComponentModel;
using System.Windows;

namespace RocketLeagueStats
{
    /// <summary>
    /// Interaction logic for ServiceWindow.xaml
    /// </summary>
    public partial class ServiceWindow : Window
    {
        private bool dontClose;

        public bool DontClose
        {
            get => dontClose;
            set
            {
                dontClose = value;
                if (value)
                    Closing += NavigatorWindow_Closing;
                else
                    Closing -= NavigatorWindow_Closing;
            }
        }

        public void NavigatorWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        public ServiceWindow()
        {
            InitializeComponent();
            DontClose = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
