using RLStats_Classes.MainClasses;
using RLStats_Classes.MainClasses.Interfaces;
using RLStats_Classes.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.AccessControl;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RocketLeagueStats
{
    /// <summary>
    /// Interaktionslogik für NavigatorWindow.xaml
    /// </summary>
    public partial class NavigatorWindow : Window, INotifyPropertyChanged
    {
        private bool _dontClose;
        private List<Replay> _shownReplays;

        public event EventHandler<(List<Replay>, List<Replay>)> GetReplaysClicked;
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly IAuthTokenInfo _tokenInfo;
        private readonly List<IReplayProvider> _providers = new();

        public List<Replay> ShownReplays
        {
            get { return _shownReplays; }
            set
            {
                _shownReplays = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShownReplays)));
            }
        }
        public List<Replay> TempReplays { get; private set; }
        public List<Replay> TempReplaysToCompare { get; private set; }
        public bool DontClose
        {
            get => _dontClose;
            set
            {
                _dontClose = value;
                if (value)
                    Closing += NavigatorWindow_Closing;
                else
                    Closing -= NavigatorWindow_Closing;
            }
        }

        public NavigatorWindow(IAuthTokenInfo tokenInfo)
        {
            _tokenInfo = tokenInfo;
            DataContext = this;
            InitializeComponent();
            //_replayProvider.DownloadProgressUpdated += Connection_DownloadProgressUpdated;
            DontClose = true;
        }

        //private void Connection_DownloadProgressUpdated(object sender, IDownloadProgress e)
        //{
        //    Dispatcher.Invoke(() =>
        //    {
        //        tbReplayCount.Text = e.ChunksToDownload.ToString();
        //        tbMessages.Text = e.DownloadMessage;
        //        if (!(e.ChunksToDownload.Equals(0) || e.PacksToDownload.Equals(0)))
        //            if (e.Initial)
        //            {
        //                loadingGrid.Clear();
        //                loadingGrid.InitializeGrid(e.PacksToDownload, e.ChunksToDownload);
        //            }
        //            else
        //            {
        //                loadingGrid.AddChunk(Brushes.Gray);
        //                var notLoadedCount = e.PacksToDownload - e.DownloadedPacks;
        //                for (int i = 0; i < e.DownloadedPacks; i++)
        //                    loadingGrid.AddPack(Brushes.GreenYellow);
        //                for (int i = 0; i < notLoadedCount; i++)
        //                    loadingGrid.AddPack(Brushes.OrangeRed);
        //            }
        //    });
        //}

        private async void BtnGetReplays_Click(object sender, RoutedEventArgs e)
        {
            if (TempReplays is null)
                await LoadReplaysIntoTemp();

            GetReplaysClicked?.Invoke(this, (TempReplays, TempReplaysToCompare));
            Hide();
        }

        public void NavigatorWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private async void BtnFetch_Click(object sender, RoutedEventArgs e)
        {
            await LoadReplaysIntoTemp();
        }

        private async Task LoadReplaysIntoTemp()
        {
            TempReplays = null;
            TempReplaysToCompare = null;
            var provider1 = new ReplayProvider(_tokenInfo);
            _providers.Add(provider1);
            var task = DownloadReplays(provider1, rpcReplayPicker.RequestFilter);
            if (!RpcReplayToComparePicker.IsEmpty)
            {
                var provider2 = new ReplayProvider(_tokenInfo);
                _providers.Add(provider2);
                TempReplaysToCompare = await DownloadReplays(provider2, RpcReplayToComparePicker.RequestFilter);
                _providers.Remove(provider2);
            }
            TempReplays = await task;
            _providers.Remove(provider1);
        }

        private async Task<List<Replay>> DownloadReplays(IReplayProvider provider, APIRequestFilter filter)
        {
            ClearTextBoxes();
            provider.DownloadProgressUpdated += (sender, e) =>
            {
                tbMessages.Text = e.CurrentMessage;
            };
            var response = await provider.CollectReplaysAsync(filter);
            ShownReplays = new List<Replay>(response.Replays);

            tbMessages.Text = (response.ElapsedMilliseconds / 1000).ToString("0.##") +
                              $" seconds";
            return new List<Replay>(response.Replays);
        }

        private void ClearTextBoxes()
        {
            tbReplayCount.Text = string.Empty;
            tbMessages.Text = string.Empty;
            tbDownloadedReplayCount.Text = string.Empty;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            foreach (var provider in _providers)
            {
                provider.CancelDownload();
            }
        }
    }
}
