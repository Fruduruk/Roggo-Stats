using RLStats_Classes.AdvancedModels;
using RLStats_Classes.MainClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RLStats_Classes.MainClasses.Interfaces;

namespace RocketLeagueStats.Components
{
    /// <summary>
    /// Interaction logic for ComparePlayersControlPage.xaml
    /// </summary>
    public partial class ComparePlayersControlPage : UserControl, IRLSControlPage
    {
        public event EventHandler<string> NotificationMessageTriggered;
        public IStatsComparer _comparer;

        public List<AdvancedReplay> AdvancedReplays { get; set; }

        public ComparePlayersControlPage()
        {
            _comparer = new StatsComparer();
            InitializeComponent();
        }

        private void Notify(string message)
        {
            NotificationMessageTriggered?.Invoke(this, message);
        }

        private async void BtnPlayerCompareClickAsync(object sender, RoutedEventArgs e)
        {
            if (!lvNamesSteamIDs.Items.IsEmpty)
            {
                var names = new List<string>();
                foreach (string s in lvNamesSteamIDs.Items)
                {
                    if (!names.Contains(s))
                        names.Add(s);
                }
                var averagePlayerStatList = await _comparer.GetAveragesAsync(AdvancedReplays, names);
                chartDisplay.AvgPlayerStatList = averagePlayerStatList;
                chartDisplay.Refresh();
            }
            else
            {
                Notify("Please add a name");
            }
        }

        private void LvNamesSteamIDs_KeyDown(object sender, KeyEventArgs e)
        {
            if (lvNamesSteamIDs.SelectedIndex >= 0)
                if (e.Key == Key.Delete)
                    lvNamesSteamIDs.Items.Remove(lvNamesSteamIDs.SelectedItem);
        }

        private void BtnAddNameOrSteamID_Click(object sender, RoutedEventArgs e) => AddNameToNameAndSteamIdListView();

        private void TbxAddNameOrSteamID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                AddNameToNameAndSteamIdListView();
        }
        private void AddNameToNameAndSteamIdListView()
        {
            if (tbxAddNameOrSteamID.Text.Trim() != string.Empty)
            {
                var name = tbxAddNameOrSteamID.Text.Trim();
                tbxAddNameOrSteamID.Text = string.Empty;
                lvNamesSteamIDs.Items.Add(name);
            }
        }
    }
}
