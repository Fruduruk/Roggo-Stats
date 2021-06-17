using RLStats_Classes.AdvancedModels;
using RLStats_Classes.MainClasses;
using RLStats_Classes.MainClasses.Interfaces;
using RLStats_Classes.Models;
using RocketLeagueStats.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace RocketLeagueStats
{
    public partial class AdvancedInfoWindow : Window
    {
        private bool dontClose;
        private List<AdvancedReplay> _advancedReplays = new List<AdvancedReplay>();
        private List<AdvancedReplay> _advancedReplaysToCompare = new List<AdvancedReplay>();
        private readonly IAdvancedReplayProvider _advancedReplayProvider;
        private List<IRLSControlPage> ControlPages { get; set; }
        public List<AdvancedReplay> AdvancedReplays
        {
            get => _advancedReplays;
            set
            {
                _advancedReplays = value;
                foreach (var page in ControlPages)
                {
                    page.AdvancedReplays = value;
                }
            }
        }

        public List<AdvancedReplay> AdvancedReplaysToCompare
        {
            get => _advancedReplaysToCompare;
            set
            {
                _advancedReplaysToCompare = value;
                foreach (var page in ControlPages)
                {
                    page.AdvancedReplaysToCompare = value;
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
            AdvancedReplaysToCompare.Clear();
            GC.Collect();
            this.Hide();
        }
        public AdvancedInfoWindow(IAdvancedReplayProvider advancedReplayProvider)
        {
            _advancedReplayProvider = advancedReplayProvider;
            InitializeComponent();
            DontClose = true;
            //AdvancedReplayProvider.AdvancedDownloadProgressUpdated += Connection_AdvancedDownloadProgressUpdated;
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
        }

        //private void Connection_AdvancedDownloadProgressUpdated(object sender, IAdvancedDownloadProgress e)
        //{
        //    Dispatcher.Invoke(() =>
        //    {
        //        tbInfo.Text = e.DownloadMessage;
        //        if (!e.ReplaysToDownload.Equals(0))
        //            if (e.Initial)
        //            {
        //                loadingGrid.Clear();
        //                loadingGrid.InitializeGrid(0, e.ReplaysToDownload);
        //            }
        //            else
        //            {
        //                loadingGrid.AddChunk(Brushes.GreenYellow);
        //            }
        //    });
        //}

        private void Page_NotificationMessageTriggered(object sender, string e)
        {
            tbInfo.Text = e;
        }

        public async void LoadReplaysAsync(IEnumerable<Replay> replays, IEnumerable<Replay> replaysToCompare = null)
        {
            var iList = await _advancedReplayProvider.GetAdvancedReplayInfosAsync(new List<Replay>(replays));
            AdvancedReplays = iList.ToList();
            if (replaysToCompare is not null)
            {
                iList = await _advancedReplayProvider.GetAdvancedReplayInfosAsync(new List<Replay>(replaysToCompare));
                AdvancedReplaysToCompare = iList.ToList();
            }
        }
    }
}
