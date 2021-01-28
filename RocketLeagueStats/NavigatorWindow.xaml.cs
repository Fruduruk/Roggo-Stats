using RLStats_Classes.Enums;
using RLStats_Classes.MainClasses;
using RLStats_Classes.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

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
            InitializeSeasonComboBox();
            InitializeDatePickers();
            InitializePlaylistCoboxBox();
            InitializeMatchResultCombobox();
            InitializeProCombobox();
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

        #region Initializer
        private void InitializeDatePickers()
        {
            dpTimeStart.SelectedDate = DateTime.Today;
            dpTimeEnd.SelectedDate = DateTime.Today;
        }
        private void InitializeProCombobox()
        {
            cbxPro.Items.Add(true);
            cbxPro.Items.Add(false);
            cbxPro.SelectedIndex = 0;
        }
        private void InitializeMatchResultCombobox()
        {
            cbxMatchResult.Items.Add(MatchResult.Win);
            cbxMatchResult.Items.Add(MatchResult.Loss);
            cbxMatchResult.SelectedIndex = 0;
        }

        private void InitializePlaylistCoboxBox()
        {
            var playlists = Enum.GetValues(typeof(Playlist));
            foreach (var p in playlists)
                cbxPlaylist.Items.Add(p);
            cbxPlaylist.SelectedIndex = 0;
        }

        private void InitializeSeasonComboBox()
        {
            for (int i = 1; i <= RLConstants.CurrentSeason; i++)
                cbxSeason.Items.Add(i);
            cbxSeason.SelectedItem = RLConstants.CurrentSeason;
        }
        #endregion
        #region checkboxes
        private void CbName_Click(object sender, RoutedEventArgs e)
        {
            var vis = GetVisibility(cbName.IsChecked);
            tbName.Visibility = vis;
            lvNames.Visibility = vis;
            btnAddName.Visibility = vis;
            tbxAddName.Visibility = vis;
        }
        private void CbSteamID_Click(object sender, RoutedEventArgs e)
        {
            var vis = GetVisibility(cbSteamID.IsChecked);
            tbSteamID.Visibility = vis;
            lvSteamIDs.Visibility = vis;
            btnAddSteamID.Visibility = vis;
            tbxAddSteamID.Visibility = vis;
        }
        private void CbDate_Click(object sender, RoutedEventArgs e)
        {
            var vis = GetVisibility(cbDate.IsChecked);
            dpTimeStart.Visibility = vis;
            dpTimeEnd.Visibility = vis;
        }
        private void CbTitle_Click(object sender, RoutedEventArgs e) => tbTitle.Visibility = GetVisibility(cbTitle.IsChecked);

        private void CbPlaylist_Click(object sender, RoutedEventArgs e) => cbxPlaylist.Visibility = GetVisibility(cbPlaylist.IsChecked);

        private void CbSeason_Click(object sender, RoutedEventArgs e) => cbxSeason.Visibility = GetVisibility(cbSeason.IsChecked);

        private void CbMatchResult_Click(object sender, RoutedEventArgs e) => cbxMatchResult.Visibility = GetVisibility(cbMatchResult.IsChecked);

        private void CbPro_Click(object sender, RoutedEventArgs e) => cbxPro.Visibility = GetVisibility(cbPro.IsChecked);

        #endregion
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
                dataPack = await Connection.Instance.CollectReplaysAsync(GetApiUrlBuilder());
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
            var dataPack = await Connection.Instance.CollectReplaysAsync(GetApiUrlBuilder());
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

        private APIUrlBuilder GetApiUrlBuilder()
        {
            var builder = GetBuilderWithoutNameAndSteamID();
            if (cbName.IsChecked == true || cbSteamID.IsChecked == true)
            {

                if (tbName.Text.Trim() != string.Empty)
                    builder.SetPlayerName(tbName.Text);
                foreach (string s in lvNames.Items)
                    builder.SetPlayerName(s);

                if (tbSteamID.Text.Trim() != string.Empty)
                    builder.SetSteamID(tbSteamID.Text);
                foreach (string s in lvSteamIDs.Items)
                    builder.SetSteamID(s);

            }
            return builder;
        }

        private APIUrlBuilder GetBuilderWithoutNameAndSteamID()
        {
            APIUrlBuilder builder = new APIUrlBuilder();
            if (cbTitle.IsChecked == true)
                if (tbTitle.Text.Trim() != string.Empty)
                    builder.SetTitle(tbTitle.Text);
            if (cbPlaylist.IsChecked == true)
                builder.SetPlaylist((Playlist)cbxPlaylist.SelectedItem);
            if (cbSeason.IsChecked == true)
                builder.SetSeason((int)cbxSeason.SelectedItem);
            if (cbMatchResult.IsChecked == true)
                builder.SetMatchResult((MatchResult)cbxMatchResult.SelectedItem);
            if (cbPro.IsChecked == true)
                builder.SetPro((bool)cbxPro.SelectedItem);
            if (cbDate.IsChecked == true)
            {
                if (dpTimeStart.SelectedDate.HasValue && dpTimeEnd.SelectedDate.HasValue)
                {
                    if (dpTimeEnd.SelectedDate.Value < dpTimeStart.SelectedDate.Value)
                    {
                        dpTimeEnd.SelectedDate = dpTimeStart.SelectedDate;
                    }
                    builder.SetStartDate(dpTimeStart.SelectedDate.Value);
                    builder.SetEndDate(dpTimeEnd.SelectedDate.Value);
                }
            }
            return builder;
        }
        #region names&steamID methods
        private void BtnAddName_Click(object sender, RoutedEventArgs e) => AddNameToNamesListView();

        private void AddNameToNamesListView()
        {
            if (tbxAddName.Text.Trim() != string.Empty)
            {
                var name = tbxAddName.Text.Trim();
                tbxAddName.Text = string.Empty;
                lvNames.Items.Add(name);
            }
        }

        private void LvNames_KeyDown(object sender, KeyEventArgs e)
        {
            if (lvNames.SelectedIndex >= 0)
                if (e.Key == Key.Delete)
                    lvNames.Items.Remove(lvNames.SelectedItem);
        }

        private void TbxAddName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                AddNameToNamesListView();
        }

        private void LvSteamIDs_KeyDown(object sender, KeyEventArgs e)
        {
            if (lvSteamIDs.SelectedIndex >= 0)
                if (e.Key == Key.Delete)
                    lvSteamIDs.Items.Remove(lvSteamIDs.SelectedItem);
        }

        private void AddNameToSteamIDListView()
        {
            if (tbxAddSteamID.Text.Trim() != string.Empty)
            {
                var name = tbxAddSteamID.Text.Trim();
                tbxAddSteamID.Text = string.Empty;
                lvSteamIDs.Items.Add(name);
            }
        }

        private void BtnAddSteamID_Click(object sender, RoutedEventArgs e) => AddNameToSteamIDListView();

        private void TbxSteamID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                AddNameToSteamIDListView();
        }
        #endregion
        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Connection.Instance.Cancel = true;


    }
}
