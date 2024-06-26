﻿using RLStatsClasses;
using RLStatsClasses.Interfaces;
using RLStatsClasses.Models;

using System;
using System.Collections.ObjectModel;
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
        public AuthTokenInfo AuthTokenInfo { get; }
        private bool dontClose;
        private IServiceInfoIO serviceInfoIO = DBProvider.Instance.GetServiceInfoDB();

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
        public ObservableCollection<APIRequestFilter> Filters { get; set; } = new ObservableCollection<APIRequestFilter>();
        public double CycleIntervalInHours
        {
            get => IsDouble(hoursToWaitAfterCycleTextBox.Text) ? Convert.ToDouble(hoursToWaitAfterCycleTextBox.Text) : 0;

            set => hoursToWaitAfterCycleTextBox.Text = value.ToString();
        }

        private bool IsDouble(string text)
        {
            bool hasComma = false;

            foreach (var c in text)
            {
                if (!char.IsDigit(c))
                {
                    if (c.Equals(',') && !hasComma)
                        hasComma = true;
                    else return false;
                }
            }
            return true;
        }

        public APIRequestFilter SelectedFilter { get; set; }
        public void ServiceWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        public ServiceWindow(AuthTokenInfo authTokenInfo)
        {
            AuthTokenInfo = authTokenInfo;
            InitializeComponent();
            lvFilters.ItemsSource = Filters;
            rpReplayPicker.gRuleName.Visibility = Visibility.Visible;
            DontClose = true;
            Activated += ServiceWindow_Activated;
            Deactivated += ServiceWindow_Deactivated;
        }

        private void ServiceWindow_Deactivated(object sender, System.EventArgs e)
        {
            SaveCurrentFilter();
        }

        private void ServiceWindow_Activated(object sender, System.EventArgs e)
        {
            LoadFilters();
        }

        private void LoadFilters()
        {
            var info = serviceInfoIO.GetServiceInfo();
            if (info.Available)
            {
                CycleIntervalInHours = info.CycleIntervalInHours;
                Filters.Clear();
                foreach (var f in info.Filters)
                    Filters.Add(f);
                if (Filters.Count > 0)
                    SelectFilter(Filters[0]);
            }
        }

        private void BtnAddFilter_Click(object sender, RoutedEventArgs e)
        {
            var filter = new APIRequestFilter
            {
                FilterName = "Filter"
            };
            Filters.Add(filter);
            SelectFilter(filter);
        }

        private void SelectFilter(APIRequestFilter filter)
        {
            SelectedFilter = filter;
            rpReplayPicker.RequestFilter = filter;
            lvFilters.SelectedItem = filter;
        }

        private void BtnDeleteFilter_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFilter != null)
            {
                var lastIndex = Filters.IndexOf(SelectedFilter);
                Filters.Remove(SelectedFilter);
                if (Filters.Count > 0)
                    if (lastIndex < Filters.Count)
                        SelectFilter(Filters[lastIndex]);
                    else
                        SelectFilter(Filters[^1]);
            }
        }

        private void LvFilters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvFilters.SelectedIndex != -1)
            {
                var filter = (APIRequestFilter)lvFilters.SelectedItem;
                SelectFilter(filter);
            }
        }

        private void LvFilters_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            SaveCurrentFilter();
        }

        private void SaveCurrentFilter()
        {
            if (Filters.Count > 0)
                if (lvFilters.SelectedIndex >= 0)
                {
                    var index = lvFilters.SelectedIndex;
                    Filters[index] = rpReplayPicker.RequestFilter;
                    lvFilters.SelectedIndex = index;
                    SaveServiceInfo();
                }
        }

        private void SaveServiceInfo()
        {
            var info = new ServiceInfo
            {
                CycleIntervalInHours = CycleIntervalInHours,
                Available = true,
                Filters = Filters,
                TokenInfo = new ServiceTokenInfo(AuthTokenInfo.Token)
                {
                    Type = AuthTokenInfo.Type
                }
            };
            serviceInfoIO.SaveServiceInfo(info);
        }
    }
}
