using RLStats_Classes.Enums;
using RLStats_Classes.MainClasses;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace RocketLeagueStats.Components
{
    /// <summary>
    /// Interaction logic for ReplayPickerControl.xaml
    /// </summary>
    public partial class ReplayPickerControl : UserControl
    {
        private const char Separator = ' ';

        public List<string> Names => GetNames();
        public List<string> SteamIDs => GetSteamIDs();
        public string Title => tbTitle.Text.Trim();
        public Playlist Playlist => (Playlist)cbxPlaylist.SelectedItem;
        public int Season => (int)cbxSeason.SelectedItem;
        public bool? Free2Play => cbSeasonType.IsChecked;
        public MatchResult MatchResult => (MatchResult)cbxMatchResult.SelectedItem;
        public bool? Pro => cbPro.IsChecked;
        

        public ReplayPickerControl()
        {
            InitializeComponent();
            InitializeSeasonComboBox(true);
            InitializeDatePickers();
            InitializePlaylistCoboxBox();
            InitializeMatchResultCombobox();
            InitializeProCombobox();
        }
        private List<string> GetSteamIDs()
        {
            var idBox = tbSteamID.Text.Trim();
            var idArray = idBox.Split(Separator);
            var ids = new List<string>();
            foreach (var s in idArray)
                if (!s.Trim().Equals(string.Empty))
                    ids.Add(s);
            return ids;
        }
        private List<string> GetNames()
        {
            var namebox = tbName.Text.Trim();
            var nameArray = namebox.Split(Separator);
            var names = new List<string>();
            foreach (var s in nameArray)
                if (!s.Trim().Equals(string.Empty))
                    names.Add(s);
            return names;
        }

        public APIUrlBuilder GetBuilder()
        {
            APIUrlBuilder builder = new APIUrlBuilder();

            if (cbName.IsChecked == true)
                foreach (var name in GetNames())
                    builder.SetPlayerName(name);

            if (cbSteamID.IsChecked == true)
                foreach (var id in GetSteamIDs())
                    builder.SetSteamID(id);

            if (cbTitle.IsChecked == true)
                if (tbTitle.Text.Trim() != string.Empty)
                    builder.SetTitle(tbTitle.Text);

            if (cbPlaylist.IsChecked == true)
                builder.SetPlaylist((Playlist)cbxPlaylist.SelectedItem);

            switch (cbSeasonType.IsChecked)
            {
                case true:
                case null:
                    builder.FreeToPlaySeason = true;
                    break;
                case false:
                    builder.FreeToPlaySeason = false;
                    break;
            }

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
        #region checkboxes
        private void CbName_Click(object sender, RoutedEventArgs e)
        {
            var vis = GetVisibility(cbName.IsChecked);
            tbName.Visibility = vis;
        }
        private void CbSteamID_Click(object sender, RoutedEventArgs e)
        {
            var vis = GetVisibility(cbSteamID.IsChecked);
            tbSteamID.Visibility = vis;
        }
        private void CbDate_Click(object sender, RoutedEventArgs e)
        {
            var vis = GetVisibility(cbDate.IsChecked);
            dpTimeStart.Visibility = vis;
            dpTimeEnd.Visibility = vis;
        }
        private void CbSeasonType_Click(object sender, RoutedEventArgs e) => InitializeSeasonComboBox(cbSeasonType.IsChecked ?? true);

        private void CbTitle_Click(object sender, RoutedEventArgs e) => tbTitle.Visibility = GetVisibility(cbTitle.IsChecked);

        private void CbPlaylist_Click(object sender, RoutedEventArgs e) => cbxPlaylist.Visibility = GetVisibility(cbPlaylist.IsChecked);

        private void CbSeason_Click(object sender, RoutedEventArgs e) => spSeason.Visibility = GetVisibility(cbSeason.IsChecked);

        private void CbMatchResult_Click(object sender, RoutedEventArgs e) => cbxMatchResult.Visibility = GetVisibility(cbMatchResult.IsChecked);

        private void CbPro_Click(object sender, RoutedEventArgs e) => cbxPro.Visibility = GetVisibility(cbPro.IsChecked);

        
        private Visibility GetVisibility(bool? isChecked)
        {
            return isChecked switch
            {
                true => Visibility.Visible,
                false => Visibility.Collapsed,
                _ => Visibility.Collapsed,
            };
        }

        #endregion
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
        private void InitializeSeasonComboBox(bool free2Play)
        {
            var season = ((free2Play) ? RLConstants.CurrentSeason : 14);
            cbxSeason.Items.Clear();
            for (int i = 1; i <= season; i++)
                cbxSeason.Items.Add(i);
            cbxSeason.SelectedItem = season;
        }

        #endregion
    }
}
