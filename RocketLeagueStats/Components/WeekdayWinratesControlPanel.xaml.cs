﻿using RocketLeagueStats.AdvancedModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RocketLeagueStats.Components
{
    /// <summary>
    /// Interaction logic for WeekdayWinratesControlPanel.xaml
    /// </summary>
    public partial class WeekdayWinratesControlPanel : UserControl, IRLSControlPage
    {
        public event EventHandler<string> NotificationMessageTriggered;
        public AdvancedLogic Logic { get; set; }
        public List<AdvancedReplay> AdvancedReplays { get; set; }
        public WeekdayWinratesControlPanel()
        {
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
                var mapWinrates = Logic.CalculateWeekDayWinrates(AdvancedReplays, tbxNameOrId.Text.Trim());
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
