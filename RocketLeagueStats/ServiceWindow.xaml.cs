using RLStats_Classes.MainClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace RocketLeagueStats
{
    /// <summary>
    /// Interaction logic for ServiceWindow.xaml
    /// </summary>
    public partial class ServiceWindow : Window
    {
        private bool dontClose;
        public bool DontClose
        {
            get => dontClose;
            set
            {
                dontClose = value;
                if (value)
                    Closing += ServiceWindow_Closing;
                else
                    Closing -= ServiceWindow_Closing;
            }
        }

        public void ServiceWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        public ServiceWindow()
        {
            InitializeComponent();

            DontClose = true;
        }

        private void BtnAddRule_Click(object sender, RoutedEventArgs e)
        {
            var filter = new APIRequestFilter();
            lvRules.Items.Add(filter);
            lvRules.SelectedItem = filter;
        }

        private void BtnDeleteRule_Click(object sender, RoutedEventArgs e)
        {
            if (lvRules.SelectedIndex != -1)
                lvRules.Items.Remove(lvRules.Items[lvRules.SelectedIndex]);
        }

        private void LvRules_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvRules.SelectedIndex != -1)
                ShowRule(lvRules.SelectedItem);
        }

        private void ShowRule(object selectedItem)
        {
            if (selectedItem is APIRequestFilter rule)
            {
                rpReplayPicker.SetNames(rule.CheckName ? rule.Names : null);
                rpReplayPicker.SetTitle(rule.CheckTitle ? rule.Title : null);
                if (rule.CheckPlaylist)
                    rpReplayPicker.SetPlaylist(rule.Playlist);
                else
                    rpReplayPicker.SetPlaylist(null);
                if (rule.CheckMatchResult)
                    rpReplayPicker.SetMatchResult(rule.MatchResult);
                else
                    rpReplayPicker.SetMatchResult(null);
                if (rule.CheckSeason)
                    rpReplayPicker.SetSeason(rule.Season);
                else
                    rpReplayPicker.SetSeason(null);
                rpReplayPicker.SetPro(rule.Pro);
                rpReplayPicker.SetSteamIDs(rule.CheckSteamID ? rule.SteamIDs : null);
                rpReplayPicker.SetFree2Play(rule.FreeToPlaySeason);
                if (rule.CheckDate)
                    rpReplayPicker.SetDateRange(rule.DateRange.Item1, rule.DateRange.Item2);
                else
                    rpReplayPicker.SetDateRange(null, null);

            }
        }
    }
}
