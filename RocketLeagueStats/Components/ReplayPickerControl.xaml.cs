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
        #region Properties
        private List<string> GetNames()
        {
            if (cbName.IsChecked == false)
                return null;
            var namebox = tbName.Text.Trim();
            var nameArray = namebox.Split(Separator);
            var names = new List<string>();
            foreach (var s in nameArray)
                if (!s.Trim().Equals(string.Empty))
                    names.Add(s);
            return names;
        }
        public void SetNames(List<string> names)
        {
            cbName.IsChecked = names != null;
            tbName.Text = string.Empty;
            foreach (var name in names)
            {
                tbName.Text += name + " ";
            }
        }
        private List<string> GetSteamIDs()
        {
            if (cbSteamID.IsChecked == false)
                return null;
            var idBox = tbSteamID.Text.Trim();
            var idArray = idBox.Split(Separator);
            var ids = new List<string>();
            foreach (var s in idArray)
                if (!s.Trim().Equals(string.Empty))
                    ids.Add(s);
            return ids;
        }
        public void SetSteamIDs(List<string> ids)
        {
            cbSteamID.IsChecked = ids != null;
            tbSteamID.Text = string.Empty;
            foreach (var id in ids)
            {
                tbName.Text += id + " ";
            }
        }
        public string GetTitle()
        {
            if (cbTitle.IsChecked == false)
                return null;
            return tbTitle.Text.Trim();
        }
        public void SetTitle(string title)
        {
            cbTitle.IsChecked = title != null;
            tbTitle.Text = title;
        }
        public Playlist? GetPlaylist()
        {
            if (cbPlaylist.IsChecked == false)
                return null;
            return (Playlist)cbxPlaylist.SelectedItem; ;
        }
        public void SetPlaylist(Playlist? playlist)
        {
            cbPlaylist.IsChecked = playlist != null;
            cbxPlaylist.SelectedItem = playlist;
        }
        public int? GetSeason()
        {
            if (cbSeason.IsChecked == false)
                return null;
            return (int)cbxSeason.SelectedItem;
        }
        public void SetSeason(int? season)
        {
            cbSeason.IsChecked = season != null;
            cbxSeason.SelectedItem = season;
        }
        public bool? GetFree2Play()
        {
            if (cbSeason.IsChecked == false)
                return null;
            return cbSeasonType.IsChecked ?? true;
        }
        public void SetFree2Play(bool isFree2Play)
        {
            cbSeasonType.IsChecked = isFree2Play;
        }
        public MatchResult? GetMatchResult()
        {
            if (cbMatchResult.IsChecked == false)
                return null;
            return (MatchResult)cbxMatchResult.SelectedItem;
        }
        public void SetMatchResult(MatchResult? matchResult)
        {
            cbMatchResult.IsChecked = matchResult != null;
            cbxMatchResult.SelectedItem = matchResult;
        }
        public bool GetPro()
        {
            return cbPro.IsChecked ?? false;
        }
        public void SetPro(bool hasProInIt)
        {
            cbPro.IsChecked = hasProInIt;
        }
        public Tuple<DateTime, DateTime> GetDateRange()
        {
            if (cbDate.IsChecked == true)
                return new Tuple<DateTime, DateTime>(dpTimeStart.SelectedDate ?? DateTime.Today, dpTimeEnd.SelectedDate ?? DateTime.Today);
            else if (cbDate.IsChecked == false)
                return null;
            else
                return null;
        }
        public void SetDateRange(DateTime? startDate, DateTime? endDate)
        {
            if (startDate is null || endDate is null)
                cbDate.IsChecked = false;
            else
                cbDate.IsChecked = true;
            dpTimeStart.SelectedDate = startDate;
            dpTimeEnd.SelectedDate = endDate;
        }

        #endregion
        public ReplayPickerControl()
        {
            InitializeComponent();
            InitializeSeasonComboBox(true);
            InitializeDatePickers();
            InitializePlaylistCoboxBox();
            InitializeMatchResultCombobox();
            InitializeProCombobox();
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
        private void InitializeSeasonComboBox(bool free2Play)
        {
            var season = ((free2Play) ? RLConstants.CurrentSeason : 14);
            cbxSeason.Items.Clear();
            for (int i = 1; i <= season; i++)
                cbxSeason.Items.Add(i);
            cbxSeason.SelectedItem = season;
        }
        #endregion
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
        public APIRequestFilter GetRequestFilter()
        {
            APIRequestFilter filter = new APIRequestFilter();
            filter.CheckName = cbName.IsChecked ?? false;
            filter.Names = GetNames();

            filter.CheckTitle = cbTitle.IsChecked ?? false;
            filter.Title = tbTitle.Text;

            filter.CheckPlaylist = cbPlaylist.IsChecked ?? false;
            filter.Playlist = (Playlist)cbxPlaylist.SelectedItem;

            filter.FreeToPlaySeason = cbSeasonType.IsChecked ?? true;

            filter.CheckSeason = cbSeason.IsChecked ?? false;
            filter.Season = (int)cbxSeason.SelectedItem;

            filter.CheckMatchResult = cbMatchResult.IsChecked ?? false;
            filter.MatchResult = (MatchResult)cbxMatchResult.SelectedItem;

            filter.Pro = (bool)cbxPro.SelectedItem;

            filter.CheckSteamID = cbSteamID.IsChecked ?? false;
            filter.SteamIDs = GetSteamIDs();

            filter.CheckDate = cbDate.IsChecked ?? false;
            if (cbDate.IsChecked == true)
            {
                if (dpTimeStart.SelectedDate.HasValue && dpTimeEnd.SelectedDate.HasValue)
                {
                    if (dpTimeEnd.SelectedDate.Value < dpTimeStart.SelectedDate.Value)
                    {
                        dpTimeEnd.SelectedDate = dpTimeStart.SelectedDate;
                    }
                    filter.DateRange = new Tuple<DateTime, DateTime>(dpTimeStart.SelectedDate.Value, dpTimeEnd.SelectedDate.Value);
                }
            }
            return filter;
        }


    }
}
