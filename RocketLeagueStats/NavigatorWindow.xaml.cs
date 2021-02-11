using RLStats_Classes.MainClasses;
using RLStats_Classes.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

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

        public NavigatorWindow()
        {
            DataContext = this;
            InitializeComponent();
            Connection.ProgressUpdated += Instance_OnProgressUpdate;
            Connection.DownloadStarted += Connection_DownloadStarted;
            Connection.ProgressChanged += Connection_ProgressChanged;
            DontClose = true;
            TempDataPack = new ApiDataPack()
            {
                Success = false,
                Ex = new Exception("There was no TempDataPack")
            };
        }

        private void Instance_OnProgressUpdate(object sender, double e)
        {
            if (0 <= e && e <= 100)
                pbDownload.Value = e;
        }
        private void Connection_ProgressChanged(object sender, string e) => tbMessages.Text = e;
        private void Connection_DownloadStarted(object sender, int e) => tbReplayCount.Text = e.ToString();

        private async void BtnGetReplays_Click(object sender, RoutedEventArgs e)
        {
            ApiDataPack dataPack;
            if (TempDataPack.Success)
                dataPack = TempDataPack;
            else
                dataPack = await Connection.Instance.CollectReplaysAsync(rpcReplayPicker.RequestFilter);
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
            var dataPack = await Connection.Instance.CollectReplaysAsync(rpcReplayPicker.RequestFilter);
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
            tbMessages.Text = (Connection.ElapsedMilliseconds / 1000).ToString("0.##") + $" seconds\nDouble Replays: {Connection.ObsoleteReplayCount}";
        }

        private void ClearTextBoxes()
        {
            tbReplayCount.Text = string.Empty;
            tbMessages.Text = string.Empty;
            tbDownloadedReplayCount.Text = string.Empty;
        }


        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Connection.Instance.Cancel = true;


    }
}
