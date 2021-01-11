using RocketLeagueStats.AdvancedModels;
using RocketLeagueStats.AverageModels;
using RocketLeagueStats.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RocketLeagueStats
{
    public partial class AdvancedInfoWindow : Window
    {
        private bool dontClose;
        private List<AdvancedReplay> advancedReplays = new List<AdvancedReplay>();
        private AdvancedLogic logic;
        private List<IRLSControlPage> ControlPages { get; set; }
        public AdvancedLogic Logic
        {
            get => logic;
            set
            {
                logic = value;
                foreach (IRLSControlPage page in ControlPages)
                {
                    page.Logic = value;
                }
            }
        }
        public List<AdvancedReplay> AdvancedReplays
        {
            get => advancedReplays;
            set
            {
                advancedReplays = value;
                foreach (var page in ControlPages)
                {
                    page.AdvancedReplays = value;
                }
            }
        }
        public bool DontClose
        {
            get => dontClose;
            set
            {
                dontClose = value;
                if (value)
                    Closing += AdvancedInfoWindow_Closing;
                else
                    Closing -= AdvancedInfoWindow_Closing;
            }
        }
        private void AdvancedInfoWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            AdvancedReplays.Clear();
            GC.Collect();
            this.Hide();
        }
        public AdvancedInfoWindow()
        {
            InitializeComponent();
            DontClose = true;
            Connection.AdvancedProgressUpdated += Connection_AdvancedProgressUpdated;
            Connection.AdvancedProgressChanged += Connection_AdvancedProgressChanged;
            ControlPages = new List<IRLSControlPage>()
            {
                mapWinratesCP,
                weekdayWinratesCP,
                comparePlayersCP
            };
            foreach(var page in ControlPages)
            {
                page.NotificationMessageTriggered += Page_NotificationMessageTriggered;
            }
            Logic = new AdvancedLogic();
        }

        private void Page_NotificationMessageTriggered(object sender, string e)
        {
            tbInfo.Text = e;
        }

        private void Connection_AdvancedProgressChanged(object sender, string e) => Dispatcher.Invoke(() => tbInfo.Text = e);

        private void Connection_AdvancedProgressUpdated(object sender, double e)
        {
            if (0 <= e && e <= 100)
                Dispatcher.Invoke(() => pbDownload.Value = e);
        }

        public async void LoadReplaysAsync(List<Replay> replays)
        {
            AdvancedReplays = await Connection.Instance.GetAdvancedReplayInfosAsync(replays);
        }
    }
}
