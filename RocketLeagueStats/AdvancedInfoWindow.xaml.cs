using RLStats_Classes.AdvancedModels;
using RLStats_Classes.MainClasses;
using RLStats_Classes.Models;
using RocketLeagueStats.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using RLStats_Classes.MainClasses.Interfaces;

namespace RocketLeagueStats
{
    public partial class AdvancedInfoWindow : Window
    {
        private bool dontClose;
        private List<AdvancedReplay> advancedReplays = new List<AdvancedReplay>();
        private AdvancedLogic logic;
        private readonly IAdvancedReplayProvider _advancedReplayProvider;
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
        public AdvancedInfoWindow(IAdvancedReplayProvider advancedReplayProvider)
        {
            _advancedReplayProvider = advancedReplayProvider;
            InitializeComponent();
            DontClose = true;
            AdvancedReplayProvider.AdvancedDownloadProgressUpdated += Connection_AdvancedDownloadProgressUpdated;
            ControlPages = new List<IRLSControlPage>()
            {
                mapWinratesCP,
                weekdayWinratesCP,
                comparePlayersCP
            };
            foreach (var page in ControlPages)
            {
                page.NotificationMessageTriggered += Page_NotificationMessageTriggered;
            }
            Logic = new AdvancedLogic();
        }

        private void Connection_AdvancedDownloadProgressUpdated(object sender, IAdvancedDownloadProgress e)
        {
            Dispatcher.Invoke(() =>
            {
                tbInfo.Text = e.DownloadMessage;
                if (!e.ReplaysToDownload.Equals(0))
                    if (e.Initial)
                    {
                        loadingGrid.Clear();
                        loadingGrid.InitializeGrid(0, e.ReplaysToDownload);
                    }
                    else
                    {
                        loadingGrid.AddChunk(Brushes.GreenYellow);
                    }
            });
        }

        private void Page_NotificationMessageTriggered(object sender, string e)
        {
            tbInfo.Text = e;
        }

        public async void LoadReplaysAsync(List<Replay> replays)
        {
            var iList = await _advancedReplayProvider.GetAdvancedReplayInfosAsync(replays);
            AdvancedReplays = iList.ToList();
        }
    }
}
