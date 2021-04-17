using RLStats_Classes.AdvancedModels;
using RLStats_Classes.MainClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RLStats_Classes.MainClasses.Interfaces;

namespace RocketLeagueStats.Components
{
    /// <summary>
    /// Interaction logic for WeekdayWinratesControlPanel.xaml
    /// </summary>
    public partial class WeekdayWinratesControlPanel : UserControl, IRLSControlPage
    {
        public event EventHandler<string> NotificationMessageTriggered;
        public List<AdvancedReplay> AdvancedReplays { get; set; }
        private readonly IStatsComparer _comparer;
        public WeekdayWinratesControlPanel()
        {
            _comparer = new StatsComparer();
            InitializeComponent();
        }
        private void Notify(string message)
        {
            NotificationMessageTriggered?.Invoke(this, message);
        }

        private void BtnWeekDayWinrates_Click(object sender, RoutedEventArgs e)
        {
            if (!tbxNameOrId.Text.Trim().Equals(string.Empty))
            {
                var mapWinrates = _comparer.CalculateWeekDayWinrates(AdvancedReplays, tbxNameOrId.Text.Trim()).ToList();
                mapWinrates.Sort();
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
