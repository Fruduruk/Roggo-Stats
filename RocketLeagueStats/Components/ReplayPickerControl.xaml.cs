﻿using RLStatsClasses;
using RLStatsClasses.Enums;

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

        public APIRequestFilter RequestFilter
        {
            get => GetRequestFilter();
            set => SetRequestFilter(value);
        }

        #region Properties
        public bool IsEmpty => GetEmptynessOfThisFilter();

        private bool GetEmptynessOfThisFilter()
        {
            if (cbName.IsChecked == true)
                return false;
            if (cbTitle.IsChecked == true)
                return false;
            if (cbPlaylist.IsChecked == true)
                return false;
            if (cbSeason.IsChecked == true)
                return false;
            if (cbMatchResult.IsChecked == true)
                return false;
            if (cbSteamID.IsChecked == true)
                return false;
            if (cbDate.IsChecked == true)
                return false;
            if (cbAlsoDownloadReplayFiles.IsChecked == true)
                return false;
            return true;
        }

        private List<string> GetNames()
        {
            if (cbName.IsChecked == false)
                return new List<string>();
            var namebox = tbName.Text.Trim();
            var nameArray = namebox.Split(Separator);
            var names = new List<string>();
            foreach (var s in nameArray)
                if (!s.Trim().Equals(string.Empty))
                    names.Add(s);
            return names;
        }
        private void SetNames(List<string> names)
        {
            if (names is null)
                return;
            tbName.Text = string.Empty;
            if (names != null)
                foreach (var name in names)
                    tbName.Text += name.Trim() + " ";
        }
        private List<string> GetSteamIDs()
        {
            if (cbSteamID.IsChecked == false)
                return new List<string>();
            else
            {
                var idBox = tbSteamID.Text.Trim();
                var idArray = idBox.Split(Separator);
                var ids = new List<string>();
                foreach (var s in idArray)
                    if (!s.Trim().Equals(string.Empty))
                        ids.Add(s);
                return ids;
            }
        }
        private void SetSteamIDs(List<string> ids)
        {
            if (ids is null)
                return;
            else
            {
                tbSteamID.Text = string.Empty;
                foreach (var id in ids)
                    tbSteamID.Text += id.Trim() + " ";
            }
        }
        private string GetTitle()
        {
            if (cbTitle.IsChecked == false)
                return string.Empty;
            else
                return tbTitle.Text.Trim();
        }
        private void SetTitle(string title)
        {
            if (title is null)
                return;
            else
                tbTitle.Text = title;
        }
        private Playlist GetPlaylist()
        {
            if (cbPlaylist.IsChecked.Equals(false))
                return 0;
            else
                return (Playlist)cbxPlaylist.SelectedItem;
        }
        private void SetPlaylist(Playlist playlist)
        {
            cbxPlaylist.SelectedItem = playlist;
        }
        private int GetSeason()
        {
            if (cbSeason.IsChecked.Equals(true))
                return (int)cbxSeason.SelectedItem;
            else
                return 0;
        }
        private void SetSeason(int season)
        {
            cbxSeason.SelectedItem = season;
        }
        private bool GetFree2Play()
        {
            if (cbSeasonType.IsChecked.Equals(true))
                return true;
            else
                return false;
        }
        private void SetFree2Play(bool isFree2Play)
        {
            cbSeasonType.IsChecked = isFree2Play;
            InitializeSeasonComboBoxAndFree2PlayCheckBox(isFree2Play);
        }
        private MatchResult GetMatchResult()
        {
            if (cbMatchResult.IsChecked.Equals(true))
                return (MatchResult)cbxMatchResult.SelectedItem;
            else
                return 0;
        }
        private void SetMatchResult(MatchResult matchResult)
        {
            cbxMatchResult.SelectedItem = matchResult;
        }
        private bool GetPro()
        {
            if (cbPro.IsChecked.Equals(true))
                return true;
            else
                return false;
        }
        private void SetPro(bool hasProInIt)
        {
            cbPro.IsChecked = hasProInIt;
        }
        private Tuple<DateTime, DateTime> GetDateRange()
        {
            if (cbDate.IsChecked.Equals(true))
                if (dpTimeStart.SelectedDate.HasValue && dpTimeEnd.SelectedDate.HasValue)
                {
                    if (dpTimeEnd.SelectedDate.Value < dpTimeStart.SelectedDate.Value)
                    {
                        dpTimeEnd.SelectedDate = dpTimeStart.SelectedDate;
                    }
                    return new Tuple<DateTime, DateTime>(dpTimeStart.SelectedDate.Value, dpTimeEnd.SelectedDate.Value);
                };
            return new Tuple<DateTime, DateTime>(dpTimeStart.SelectedDate ?? DateTime.Today, dpTimeEnd.SelectedDate ?? DateTime.Today);
        }
        private void SetDateRange(DateTime startDate, DateTime endDate)
        {
            dpTimeStart.SelectedDate = startDate;
            dpTimeEnd.SelectedDate = endDate;
        }

        private string GetReplayFilePath()
        {
            if (cbAlsoDownloadReplayFiles.IsChecked == false)
                return string.Empty;
            else
                return tbxAlsoDownloadReplayFiles.Text.Trim();
        }
        private void SetReplayFilePath(string path)
        {
            if (path is null)
                return;
            else
                tbxAlsoDownloadReplayFiles.Text = path;
        }
        #endregion
        public ReplayPickerControl()
        {
            InitializeComponent();
            SetEverythingDefault();
        }
        private void SetEverythingDefault()
        {
            cbName.IsChecked = false;
            cbTitle.IsChecked = false;
            cbPlaylist.IsChecked = false;
            cbSeason.IsChecked = false;
            cbMatchResult.IsChecked = false;
            cbSteamID.IsChecked = false;
            cbDate.IsChecked = false;
            cbAlsoDownloadReplayFiles.IsChecked = false;

            cbSeasonType.IsChecked = true;
            cbPro.IsChecked = false;
            tbName.Text = string.Empty;
            tbTitle.Text = string.Empty;
            tbSteamID.Text = string.Empty;
            InitializeSeasonComboBoxAndFree2PlayCheckBox(true);
            InitializeDatePickers();
            InitializePlaylistCoboxBox();
            InitializeMatchResultCombobox();
            InitializeProCombobox();
            tbxAlsoDownloadReplayFiles.Text = string.Empty;
            RefreshVisibilities();
        }
        private void RefreshVisibilities()
        {
            CbName_Click(null, null);
            CbTitle_Click(null, null);
            CbSeasonType_Click(null, null);
            CbPlaylist_Click(null, null);
            CbSeason_Click(null, null);
            CbMatchResult_Click(null, null);
            CbPro_Click(null, null);
            CbSteamID_Click(null, null);
            CbDate_Click(null, null);
            CbAlsoDownloadReplayFiles_Click(null, null);
        }
        #region Initializer
        private void InitializeDatePickers()
        {
            dpTimeStart.SelectedDate = DateTime.Today;
            dpTimeEnd.SelectedDate = DateTime.Today;
        }
        private void InitializeProCombobox()
        {
            cbxPro.Items.Clear();
            cbxPro.Items.Add(true);
            cbxPro.Items.Add(false);
            cbxPro.SelectedIndex = 0;
        }
        private void InitializeMatchResultCombobox()
        {
            cbxMatchResult.Items.Clear();
            cbxMatchResult.Items.Add(MatchResult.Win);
            cbxMatchResult.Items.Add(MatchResult.Loss);
            cbxMatchResult.SelectedIndex = 0;
        }
        private void InitializePlaylistCoboxBox()
        {
            var playlists = Enum.GetValues(typeof(Playlist));
            cbxPlaylist.Items.Clear();
            foreach (var p in playlists)
                cbxPlaylist.Items.Add(p);
            cbxPlaylist.SelectedIndex = 0;
        }
        private void InitializeSeasonComboBoxAndFree2PlayCheckBox(bool free2Play)
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
        private void CbSeasonType_Click(object sender, RoutedEventArgs e) => InitializeSeasonComboBoxAndFree2PlayCheckBox(cbSeasonType.IsChecked ?? true);
        private void CbTitle_Click(object sender, RoutedEventArgs e) => tbTitle.Visibility = GetVisibility(cbTitle.IsChecked);
        private void CbPlaylist_Click(object sender, RoutedEventArgs e) => cbxPlaylist.Visibility = GetVisibility(cbPlaylist.IsChecked);
        private void CbSeason_Click(object sender, RoutedEventArgs e) => spSeason.Visibility = GetVisibility(cbSeason.IsChecked);
        private void CbMatchResult_Click(object sender, RoutedEventArgs e) => cbxMatchResult.Visibility = GetVisibility(cbMatchResult.IsChecked);
        private void CbPro_Click(object sender, RoutedEventArgs e) => cbxPro.Visibility = GetVisibility(cbPro.IsChecked);
        private void CbAlsoDownloadReplayFiles_Click(object sender, RoutedEventArgs e) => tbxAlsoDownloadReplayFiles.Visibility = GetVisibility(cbAlsoDownloadReplayFiles.IsChecked);
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
        private APIRequestFilter GetRequestFilter()
        {
            APIRequestFilter filter = new APIRequestFilter();
            filter.FilterName = tbxFilterName.Text.Trim();

            filter.CheckName = cbName.IsChecked ?? false;
            if (filter.CheckName)
                filter.Names = GetNames();

            filter.CheckTitle = cbTitle.IsChecked ?? false;
            if (filter.CheckTitle)
                filter.Title = GetTitle();

            filter.CheckPlaylist = cbPlaylist.IsChecked ?? false;
            if (filter.CheckPlaylist)
                filter.Playlist = GetPlaylist();

            filter.FreeToPlaySeason = GetFree2Play();

            filter.CheckSeason = cbSeason.IsChecked ?? false;
            if (filter.CheckSeason)
                filter.Season = GetSeason();

            filter.CheckMatchResult = cbMatchResult.IsChecked ?? false;
            if (filter.CheckMatchResult)
                filter.MatchResult = GetMatchResult();

            filter.Pro = GetPro();

            filter.CheckSteamId = cbSteamID.IsChecked ?? false;
            if (filter.CheckSteamId)
                filter.SteamIDs = GetSteamIDs();

            filter.CheckDate = cbDate.IsChecked ?? false;
            if (filter.CheckDate)
                filter.DateRange = GetDateRange();

            filter.AlsoSaveReplayFiles = cbAlsoDownloadReplayFiles.IsChecked ?? false;
            if (filter.AlsoSaveReplayFiles)
                filter.ReplayFilePath = GetReplayFilePath();

            return filter;
        }

        private void SetRequestFilter(APIRequestFilter rule)
        {
            SetEverythingDefault();
            if (rule is null)
                return;
            tbxFilterName.Text = rule.FilterName;

            cbName.IsChecked = rule.CheckName;
            if (rule.CheckName)
                SetNames(rule.Names);

            cbTitle.IsChecked = rule.CheckTitle;
            if (rule.CheckTitle)
                SetTitle(rule.Title);

            cbPlaylist.IsChecked = rule.CheckPlaylist;
            if (rule.CheckPlaylist)
                SetPlaylist(rule.Playlist);

            cbMatchResult.IsChecked = rule.CheckMatchResult;
            if (rule.CheckMatchResult)
                SetMatchResult(rule.MatchResult);

            SetPro(rule.Pro);

            SetFree2Play(rule.FreeToPlaySeason);

            cbSeason.IsChecked = rule.CheckSeason;
            if (rule.CheckSeason)
                SetSeason(rule.Season);

            cbSteamID.IsChecked = rule.CheckSteamId;
            if (rule.CheckSteamId)
                SetSteamIDs(rule.SteamIDs);

            cbDate.IsChecked = rule.CheckDate;
            if (rule.CheckDate)
                SetDateRange(rule.DateRange.Item1, rule.DateRange.Item2);

            cbAlsoDownloadReplayFiles.IsChecked = rule.AlsoSaveReplayFiles;
            if (rule.AlsoSaveReplayFiles)
                SetReplayFilePath(rule.ReplayFilePath);

            RefreshVisibilities();
        }
    }
}
