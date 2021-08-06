﻿using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using RLStats_Classes.MainClasses;
using RLStats_Classes.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Replay_Download_Service
{
    public class ReplayDownloader
    {
        private const int ProgressBarLength = 90;
        private const string BlockValueString = "■";
        private static string LogPath => Path.Combine(RLConstants.RLStatsFolder, "serviceLog.txt");
        private static Stopwatch UpdateWatch { get; set; } = new Stopwatch();
        private static ILogger<Worker> Logger { get; set; }
        private static CancellationToken StoppingToken { get; set; }
        private static void LogObject(object o) => Log(JsonConvert.SerializeObject(o, Formatting.Indented));

        public static Task ExecuteServiceAsync(ILogger<Worker> logger, CancellationToken stoppingToken)
        {
            UpdateWatch.Start();
            return Task.Run(async () =>
            {
                Logger = logger;
                StoppingToken = stoppingToken;
                while (!stoppingToken.IsCancellationRequested)
                {
                    var sInfo = new ServiceInfoIO().GetServiceInfo();
                    Log($"Cycle started: {DateTime.Now}");
                    await StartDownloadCycle(sInfo);
                    Log("Cycle finished.");
                    Log($"Next cycle starts at {DateTime.Now.AddHours(1)}");
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }, stoppingToken);
        }

        private static async Task StartDownloadCycle(ServiceInfo serviceInfo)
        {
            if (!serviceInfo.Available)
            {
                Log("Service is not available. Cannot get information out of service info file");
                await Task.Delay(TimeSpan.FromMinutes(1), StoppingToken);
                return;
            }

            var replayProvider = new ReplayProvider(serviceInfo.TokenInfo);
            replayProvider.DownloadProgressUpdated += DownloadProgressUpdated;
            var advancedReplayProvider = new AdvancedReplayProvider(serviceInfo.TokenInfo);
            advancedReplayProvider.DownloadProgressUpdated += DownloadProgressUpdated;

            foreach (var filter in serviceInfo.Filters)
            {
                //await Task.Delay(TimeSpan.FromSeconds(5), StoppingToken);
                var response = await replayProvider.CollectReplaysAsync(filter);
                Log($"Collected {response.Replays.Count()} replays from filter \"{filter.FilterName}\" in {response.ElapsedMilliseconds / 1000d} seconds.");
                var watch = Stopwatch.StartNew();
                var advancedReplays = await advancedReplayProvider.GetAdvancedReplayInfosAsync(new List<Replay>(response.Replays));
                Log($"Collected {advancedReplays.Count} advanced replays from filter \"{filter.FilterName}\" in {watch.ElapsedMilliseconds / 1000d} seconds.");
                advancedReplays.Clear();
            }

            replayProvider.DownloadProgressUpdated -= DownloadProgressUpdated;
            advancedReplayProvider.DownloadProgressUpdated -= DownloadProgressUpdated;
        }

        private static void DownloadProgressUpdated(object sender, ProgressState e)
        {
            var progressBar = (!e.TotalCount.Equals(0)) ? GetProgressBar((double)e.PartCount / (double)(e.TotalCount - e.FalsePartCount)) : string.Empty;
            var outOf = $"{e.PartCount}/{e.TotalCount - e.FalsePartCount}";
            var text = $"{e.CurrentMessage} {outOf} {progressBar}";
            if (UpdateWatch.ElapsedMilliseconds > 5000 || e.LastCall)
            {
                UpdateWatch.Restart();
                Log(text, true);
            }
        }

        private static void Log(string text, bool debug = false)
        {
            if (debug)
                Logger.LogDebug(text);
            else
                Logger.LogInformation(text);
            File.AppendAllLines(LogPath, new string[] { text });
        }

        private static string GetProgressBar(double percent)
        {
            return $"|{GetBar()}|";

            string GetBar()
            {
                int blockCount = (int)Math.Truncate(ProgressBarLength * percent);
                var builder = new StringBuilder();
                for (int i = 0; i < blockCount; i++)
                    builder.Append(BlockValueString);
                for (int i = 0; i < ProgressBarLength - blockCount; i++)
                    builder.Append(" ");
                return builder.ToString();
            }
        }
    }
}