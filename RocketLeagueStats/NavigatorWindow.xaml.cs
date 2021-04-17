using RLStats_Classes.MainClasses;
using RLStats_Classes.MainClasses.Interfaces;
using RLStats_Classes.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace RocketLeagueStats
{
    /// <summary>
    /// Interaktionslogik für NavigatorWindow.xaml
    /// </summary>
    public partial class NavigatorWindow : Window, INotifyPropertyChanged
    {
        private bool dontClose;
        private List<Replay> shownReplays;

        public event EventHandler<ApiDataPack> GetReplaysClicked;
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly IReplayProvider _replayProvider;

        public List<Replay> ShownReplays
        {
            get { return shownReplays; }
            set
            {
                shownReplays = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShownReplays"));
            }
        }
        public ApiDataPack TempDataPack { get; private set; }
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

        public NavigatorWindow(IReplayProvider replayProvider)
        {
            _replayProvider = replayProvider;
            DataContext = this;
            InitializeComponent();
            ReplayProvider.DownloadProgressUpdated += Connection_DownloadProgressUpdated;
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
            ApiDataPack dataPack;
            if (TempDataPack.Success)
                dataPack = TempDataPack;
            else
                dataPack = await _replayProvider.CollectReplaysAsync(rpcReplayPicker.RequestFilter);
            GetReplaysClicked?.Invoke(this, dataPack);
            Hide();
        }

        public void NavigatorWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
        private Visibility GetVisibility(bool? isChecked)
        {
            return isChecked switch
            {
                true => Visibility.Visible,
                false => Visibility.Collapsed,
                _ => Visibility.Collapsed,
            };
        }

        private async void BtnFetch_Click(object sender, RoutedEventArgs e)
        {
            ClearTextBoxes();
            var dataPack = await _replayProvider.CollectReplaysAsync(rpcReplayPicker.RequestFilter);
            if (dataPack.Success)
            {
                tbDownloadedReplayCount.Text = dataPack.Replays.Count.ToString();
                ShownReplays = dataPack.Replays;
                TempDataPack = dataPack;
            }
            else
            {
                tbMessages.Text = dataPack.Ex.Message + "\n" + dataPack.ReceivedString;
            }
            tbMessages.Text = (ReplayProvider.ElapsedMilliseconds / 1000).ToString("0.##") + $" seconds\nDouble Replays: {ReplayProvider.ObsoleteReplayCount}";
        }

        private void ClearTextBoxes()
        {
            tbReplayCount.Text = string.Empty;
            tbMessages.Text = string.Empty;
            tbDownloadedReplayCount.Text = string.Empty;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => _replayProvider.CancelDownload();
    }
}
