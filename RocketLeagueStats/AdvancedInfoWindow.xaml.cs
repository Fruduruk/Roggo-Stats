using RLStats_Classes.MainClasses.Interfaces;
using RLStats_Classes.Models;
using RLStats_Classes.Models.Advanced;

using RocketLeagueStats.Components;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

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
            _advancedReplayProvider.DownloadProgressUpdated += AdvancedReplayProvider_DownloadProgressUpdated;
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

        private void AdvancedReplayProvider_DownloadProgressUpdated(object sender, ProgressState e)
        {
            MainLoadingGrid.UpdateView(e);
        }

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
