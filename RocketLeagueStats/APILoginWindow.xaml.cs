using System;
using RLStats_Classes.MainClasses;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace RocketLeagueStats
{
    /// <summary>
    /// Interaction logic for APILoginWindow.xaml
    /// </summary>
    public partial class APILoginWindow : Window
    {
        private readonly string _filePath = string.Empty;
        private MainWindow MW { get; set; }
        public APILoginWindow()
        {
            var args = Environment.GetCommandLineArgs();
            if (args.Length == 2)
                _filePath = args[1];
            //var api = new BallchasingAPI(RLConstants.DebugKey);
            InitializeComponent();
#if DEBUG
            try
            {
                tbxToken.Text = RLConstants.DebugKey;
                BtnLoginClick(null, null);
            }
            catch (Exception e)
            {
                tbInfo.Text = e.Message;
            }
#endif
        }
        private void MW_Closed(object sender, System.EventArgs e)
        {
            this.Close();
        }
        private void BtnLoginClick(object sender, RoutedEventArgs e)
        {
            var tokenInfo = TokenInfoProvider.GetTokenInfo(tbxToken.Text.Trim());
            if (tokenInfo.Except != null)
            {
                tbInfo.Text = tokenInfo.Except.Message;
            }
            else if (tokenInfo.Chaser)
            {
#if RELEASE
                Task.Run(() =>
                {
                    Thread.Sleep(2000);
#endif
                Dispatcher.Invoke(() =>
                {
                    this.Hide();
                    MW = new MainWindow(tokenInfo, _filePath);
                    MW.Closed += MW_Closed;
                    MW.Show();
                });
#if RELEASE
                });
#endif
                tbInfo.Text = "Token approved! Welcome " + tokenInfo.Type + " chaser";
            }
            else
            {
                tbInfo.Text = "You are not a chaser. I can't let you in.";
            }
        }
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
                e.Handled = true;
            }
            catch
            {
                tbInfo.Text = "Not implemented yet";
            }
        }
    }
}
