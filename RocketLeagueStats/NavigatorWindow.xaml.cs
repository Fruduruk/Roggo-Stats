using RLStats_Classes.MainClasses;
using RLStats_Classes.MainClasses.Interfaces;
using RLStats_Classes.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public event EventHandler<(ApiDataPack, ApiDataPack)> GetReplaysClicked;
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
        public ApiDataPack TempDataPack { get; private set; }
        public ApiDataPack TempDataPackToCompare { get; private set; }
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
            TempDataPack = new ApiDataPack()
            {
                Success = false,
                Ex = new Exception("There was no TempDataPack")
            };
        }

        private void Connection_DownloadProgressUpdated(object sender, IDownloadProgress e)
        {
            Dispatcher.Invoke(() =>
            {
                tbReplayCount.Text = e.ChunksToDownload.ToString();
                tbMessages.Text = e.DownloadMessage;
                if (!(e.ChunksToDownload.Equals(0) || e.PacksToDownload.Equals(0)))
                    if (e.Initial)
                    {
                        loadingGrid.Clear();
                        loadingGrid.InitializeGrid(e.PacksToDownload, e.ChunksToDownload);
                    }
                    else
                    {
                        loadingGrid.AddChunk(Brushes.Gray);
                        var notLoadedCount = e.PacksToDownload - e.DownloadedPacks;
                        for (int i = 0; i < e.DownloadedPacks; i++)
                            loadingGrid.AddPack(Brushes.GreenYellow);
                        for (int i = 0; i < notLoadedCount; i++)
                            loadingGrid.AddPack(Brushes.OrangeRed);
                    }
            });
        }

        private async void BtnGetReplays_Click(object sender, RoutedEventArgs e)
        {
            if (!TempDataPack.Success)
                await LoadReplaysIntoTemp();

            GetReplaysClicked?.Invoke(this, (TempDataPack, TempDataPackToCompare));
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
            TempDataPack = null;
            TempDataPackToCompare = null;
            var provider1 = new ReplayProvider(_tokenInfo);
            _providers.Add(provider1);
            var task = DownloadReplays(provider1, rpcReplayPicker.RequestFilter);
            if (!RpcReplayToComparePicker.IsEmpty)
            {
                var provider2 = new ReplayProvider(_tokenInfo);
                _providers.Add(provider2);
                TempDataPackToCompare = await DownloadReplays(provider2, RpcReplayToComparePicker.RequestFilter);
                _providers.Remove(provider2);
            }
            TempDataPack = await task;
            _providers.Remove(provider1);
        }

        private async Task<ApiDataPack> DownloadReplays(IReplayProvider provider, APIRequestFilter filter)
        {
            ClearTextBoxes();
            provider.DownloadProgressUpdated += (sender, e) =>
            {
                tbMessages.Text = e.DownloadMessage;
            };
            var response = await provider.CollectReplaysAsync(filter);
            if (response.DataPack.Success)
            {
                ShownReplays = response.DataPack.Replays;
                TempDataPack = response.DataPack;
            }
            else
            {
                tbMessages.Text = response.DataPack.Ex.Message + "\n" + response.DataPack.ReceivedString;
            }

            tbMessages.Text = (response.ElapsedMilliseconds / 1000).ToString("0.##") +
                              $" seconds\nDouble Replays: {ReplayProvider.ObsoleteReplayCount}";
            return response.DataPack;
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
