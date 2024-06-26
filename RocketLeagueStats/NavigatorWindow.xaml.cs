﻿using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using RLStatsClasses;
using RLStatsClasses.Interfaces;
using RLStatsClasses.Models;
using RLStatsClasses.Models.ReplayModels;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace RocketLeagueStats
{
    /// <summary>
    /// Interaktionslogik für NavigatorWindow.xaml
    /// </summary>
    public partial class NavigatorWindow : Window, INotifyPropertyChanged, ILogger
    {
        private bool _dontClose;
        private List<Replay> _shownReplays;

        public event EventHandler<(List<Replay>, List<Replay>)> GetReplaysClicked;
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly IAuthTokenInfo _tokenInfo;
        private readonly List<IReplayProvider> _providers = new();
        private readonly IReplayCache _replayCache = DBProvider.Instance.GetReplayCacheDB();

        public List<Replay> ShownReplays
        {
            get => _shownReplays;
            set
            {
                _shownReplays = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShownReplays)));
            }
        }
        public List<Replay> TempReplays { get; private set; }
        public List<Replay> TempReplaysToCompare { get; private set; }
        public bool DontClose
        {
            get => _dontClose;
            set
            {
                _dontClose = value;
                if (value)
                    Closing += NavigatorWindow_Closing;
                else
                    Closing -= NavigatorWindow_Closing;
            }
        }

        public NavigatorWindow(IAuthTokenInfo tokenInfo)
        {
            _tokenInfo = tokenInfo;
            DataContext = this;
            InitializeComponent();
            DontClose = true;
        }

        private async void BtnGetReplays_Click(object sender, RoutedEventArgs e)
        {
            if (TempReplays is null)
                await LoadReplaysIntoTemp();

            GetReplaysClicked?.Invoke(this, (TempReplays, TempReplaysToCompare));
            Hide();
        }

        public void NavigatorWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private async void BtnFetch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await LoadReplaysIntoTemp();
            }
            catch (Exception ex)
            {
                tbMessages.Text = ex.Message;
            }
        }

        private async Task LoadReplaysIntoTemp()
        {
            TempReplays = null;
            TempReplaysToCompare = null;
            var mainProvider = new ReplayProvider(_tokenInfo, _replayCache, this);
            mainProvider.DownloadProgressUpdated += MainProvider_DownloadProgressUpdated;
            _providers.Add(mainProvider);
            var task = DownloadReplays(mainProvider, rpcReplayPicker.RequestFilter);
            if (!RpcReplayToComparePicker.IsEmpty)
            {
                var secondProvider = new ReplayProvider(_tokenInfo, _replayCache, this);
                secondProvider.DownloadProgressUpdated += SecondProvider_DownloadProgressUpdated;
                _providers.Add(secondProvider);
                TempReplaysToCompare = await DownloadReplays(secondProvider, RpcReplayToComparePicker.RequestFilter);
                secondProvider.DownloadProgressUpdated -= SecondProvider_DownloadProgressUpdated;
                _providers.Remove(secondProvider);
            }
            TempReplays = await task;
            _providers.Remove(mainProvider);
            mainProvider.DownloadProgressUpdated -= MainProvider_DownloadProgressUpdated;
        }

        private void MainProvider_DownloadProgressUpdated(object sender, ProgressState e)
        {
            Dispatcher.Invoke(() =>
            {
                MainLoadingGrid.UpdateView(e);
                tbMessages.Text = "Provider 1: " + e.CurrentMessage;
            });
            Debug.WriteLine("Provider 1:\n" + JsonConvert.SerializeObject(e));
        }

        private void SecondProvider_DownloadProgressUpdated(object sender, ProgressState e)
        {
            Dispatcher.Invoke(() =>
            {
                SecondLoadingGrid.UpdateView(e);
                tbMessages.Text = "Provider 2: " + e.CurrentMessage;
            });
            Debug.WriteLine("Provider 2:\n" + JsonConvert.SerializeObject(e));
        }

        private async Task<List<Replay>> DownloadReplays(IReplayProvider provider, APIRequestFilter filter)
        {
            ClearTextBoxes();
            provider.DownloadProgressUpdated += (sender, e) =>
            {
                tbMessages.Text = e.CurrentMessage;
            };
            var response = await provider.CollectReplaysAsync(filter, cached: true);
            ShownReplays = new List<Replay>(response.Replays);

            tbMessages.Text = (response.ElapsedMilliseconds / 1000).ToString("0.##") +
                              $" seconds";
            return new List<Replay>(response.Replays);
        }

        private void ClearTextBoxes()
        {
            tbReplayCount.Text = string.Empty;
            tbMessages.Text = string.Empty;
            tbDownloadedReplayCount.Text = string.Empty;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            foreach (var provider in _providers)
            {
                provider.CancelDownload();
            }
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (logLevel == LogLevel.Information)
                Debug.WriteLine(state.ToString());
        }

        bool ILogger.IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }
    }
}
