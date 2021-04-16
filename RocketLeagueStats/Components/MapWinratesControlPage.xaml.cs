using RLStats_Classes.AdvancedModels;
using RLStats_Classes.MainClasses;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using RLStats_Classes.MainClasses.Interfaces;

namespace RocketLeagueStats.Components
{
    /// <summary>
    /// Interaction logic for MapWinratesControlPage.xaml
    /// </summary>
    public partial class MapWinratesControlPage : UserControl, IRLSControlPage
    {
        public event EventHandler<string> NotificationMessageTriggered;
        public List<AdvancedReplay> AdvancedReplays { get; set; }
        private IStatsComparer _comparer;
        public MapWinratesControlPage()
        {
            InitializeComponent();
            _comparer = new AdvancedLogic();
        }
        private void Notify(string message)
        {
            NotificationMessageTriggered?.Invoke(this, message);
        }
        private void BtnMapWinrates_Click(object sender, RoutedEventArgs e)
        {
            if (!tbxNameOrId.Text.Trim().Equals(string.Empty))
            {
                var mapWinrates = _comparer.CalculateMapWinRates(AdvancedReplays, tbxNameOrId.Text.Trim());

                try
                {
                    mapWinrates.Sort();
                }
                catch (Exception ex)
                {
                    Notify(ex.Message);
                }
                lvStats.Items.Clear();
                foreach (var winrate in mapWinrates)
                    lvStats.Items.Add(winrate);
            }
            else
            {
                Notify("Please set a name");
            }
        }
    }
}
