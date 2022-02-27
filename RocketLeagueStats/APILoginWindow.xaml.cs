using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;

using RLStats.MongoDBSupport;

using RLStatsClasses;

using RocketLeagueStats.Configuration;

using System;
using System.Windows;

namespace RocketLeagueStats
{
    /// <summary>
    /// Interaction logic for APILoginWindow.xaml
    /// </summary>
    public partial class APILoginWindow : Window
    {
        private readonly string filePath = string.Empty;
        private readonly ConfigHandler handler;

        private MainWindow MW { get; set; }

        public APILoginWindow()
        {
            InitializeComponent();
            handler = new ConfigHandler();
            var config = handler.LoadConfig();

            if (config is not null)
            {
                Connect(config);
                return;
            }
        }

        private void MW_Closed(object sender, EventArgs e) => Close();

        private void BtnLoginClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var useMongoDB = UseMongoDBCheckbox.IsChecked.Value;
                Config config;
                if (useMongoDB)
                    config = new Config()
                    {
                        BallchasingToken = tbxToken.Text.Trim(),
                        UseMongoDB = useMongoDB,
                        DatabaseName = DatabaseNameTextBox.Text.Trim(),
                        Host = HostTextBox.Text.Trim(),
                        Port = Convert.ToInt32(PortTextBox.Text.Trim()),
                        Username = UsernameTextBox.Text.Trim(),
                        Password = PasswordTextBox.Text
                    };
                else
                    config = new Config { BallchasingToken = tbxToken.Text.Trim() };
                handler.SaveConfig(config);
                Connect(config);
            }
            catch (Exception ex)
            {
                tbInfo.Text = ex.Message;
            }
        }

        private void Connect(Config config)
        {
            var tokenInfo = TokenInfoProvider.GetTokenInfo(config.BallchasingToken);
            if (tokenInfo.Except != null)
                throw new Exception(tokenInfo.Except.Message);
            if (!tokenInfo.Chaser)
                throw new Exception("You are not a chaser. I can't let you in.");

            SetupDB(config);
            Dispatcher.Invoke(() =>
            {
                Hide();
                MW = new MainWindow(tokenInfo, filePath);
                MW.Closed += MW_Closed;
                MW.Show();
            });
        }

        private void SetupDB(Config config)
        {
            if (!config.UseMongoDB)
            {
                DBProvider.CreateInstance();
                return;
            }
            if (string.IsNullOrWhiteSpace(config.Host))
                throw new ArgumentNullException(nameof(config.Host));
            if (config.Port.Equals(default))
                throw new ArgumentException(nameof(config.Port));
            var settings = new MongoClientSettings
            {
                Server = new MongoServerAddress(config.Host, config.Port),
                Compressors = new[] { new CompressorConfiguration(MongoDB.Driver.Core.Compression.CompressorType.Snappy) }
            };
            if (!string.IsNullOrWhiteSpace(config.Username) && !string.IsNullOrWhiteSpace(config.Password))
            {
                settings.Credential = MongoCredential.CreateCredential(config.DatabaseName, config.Username, config.Password);
            }
            DBProvider.CreateInstance(new DatabaseSettings
            {
                DatabaseName = config.DatabaseName,
                MongoSettings = settings
            });
        }

        private void UseMongoDBCheckboxChecked(object sender, RoutedEventArgs e)
        {
            DBConfigGrid.Visibility = Visibility.Visible;
        }

        private void UseMongoDBCheckboxUnchecked(object sender, RoutedEventArgs e)
        {
            DBConfigGrid.Visibility = Visibility.Collapsed;
        }
    }
}
