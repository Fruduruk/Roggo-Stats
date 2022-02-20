using RLStats.MongoDBSupport;

using RLStatsClasses;
using RLStatsClasses.Interfaces;

using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace RocketLeagueStats
{
    /// <summary>
    /// Interaction logic for APILoginWindow.xaml
    /// </summary>
    public partial class APILoginWindow : Window
    {
        private readonly string filePath = string.Empty;
        private readonly ISaveBallchasingToken tokenSaver;
        private MainWindow MW { get; set; }
        public APILoginWindow()
        {
            var args = Environment.GetCommandLineArgs();
            if (args.Length == 3)
                filePath = args[2];

            if (args.Contains("-mongo"))
            {
                DBProvider.CreateInstance(DBType.MongoDB, new DatabaseSettings
                {
                    ConnectionString = "mongodb://localhost:27017",
                    DatabaseName = "RLStatsData"
                });
            }
            else
            {
                DBProvider.CreateInstance(DBType.Legacy);
            }
            tokenSaver = DBProvider.Instance.GetBallchasingTokenDB();

            InitializeComponent();
            var key = tokenSaver.GetBallchasingToken();
            if (string.IsNullOrEmpty(key))
            {
                tbInfo.Text = "Paste your ballchasing key here";
            }
            else
            {
                tbxToken.Text = key;
                Connect();
            }
        }
        private void MW_Closed(object sender, EventArgs e) => Close();
        private void BtnLoginClick(object sender, RoutedEventArgs e) => Connect();

        private void Connect()
        {
            var tokenInfo = TokenInfoProvider.GetTokenInfo(tbxToken.Text.Trim());
            if (tokenInfo.Except != null)
            {
                tbInfo.Text = tokenInfo.Except.Message;
            }
            else if (tokenInfo.Chaser)
            {
                Dispatcher.Invoke(() =>
                {
                    Hide();
                    MW = new MainWindow(tokenInfo, filePath);
                    MW.Closed += MW_Closed;
                    MW.Show();
                });
                tbInfo.Text = "Token approved! Welcome " + tokenInfo.Type + " chaser";
                tokenSaver.SetBallchasingToken(tokenInfo.Token);
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
                var p = Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
                p.Dispose();
                e.Handled = true;
            }
            catch
            {
                tbInfo.Text = "Not implemented yet";
            }
        }
    }
}
