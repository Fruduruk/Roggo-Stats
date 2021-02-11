using RLStats_Classes.MainClasses;
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
        public ObservableCollection<APIRequestFilter> Filters { get; set; } = new ObservableCollection<APIRequestFilter>();
        public APIRequestFilter SelectedFilter { get; set; }
        public void ServiceWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        public ServiceWindow()
        {
            InitializeComponent();
            lvRules.ItemsSource = Filters;
            rpReplayPicker.gRuleName.Visibility = Visibility.Visible;
            DontClose = true;
        }

        private void BtnAddRule_Click(object sender, RoutedEventArgs e)
        {
            var filter = new APIRequestFilter
            {
                FilterName = "Rule"
            };
            Filters.Add(filter);
            SelectFilter(filter);
        }

        private void SelectFilter(APIRequestFilter filter)
        {
            SelectedFilter = filter;
            rpReplayPicker.RequestFilter = filter;
            lvRules.SelectedItem = filter;
        }

        private void BtnDeleteRule_Click(object sender, RoutedEventArgs e)
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

        private void LvRules_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvRules.SelectedIndex != -1)
                Dispatcher.Invoke(() =>
                {
                    var filter = (APIRequestFilter)lvRules.SelectedItem;
                    SelectFilter(filter);
                });
        }
    }
}
