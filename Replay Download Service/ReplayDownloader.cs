using Ionic.Zip;

using Microsoft.Extensions.Logging;

using RLStatsClasses;
using RLStatsClasses.CacheHandlers;
using RLStatsClasses.Interfaces;
using RLStatsClasses.Models;
using RLStatsClasses.Models.ReplayModels;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReplayDownloadService
{
    public static class ReplayDownloader
    {
        private const int ProgressBarLength = 90;
        private const string BlockValueString = "■";

        private static string ServiceLogsZipPath { get; set; } = Path.Combine(RLConstants.RLStatsFolder, "ServiceLogs.zip");
        private static string LogPath => Path.Combine(RLConstants.RLStatsFolder, "ServiceLog.txt");
        private static string UpdateLogPath => Path.Combine(RLConstants.RLStatsFolder, "serviceUpdateLog.txt");
        private static Stopwatch UpdateWatch { get; set; } = new Stopwatch();
        private static ILogger<Worker> Logger { get; set; }
        private static IServiceInfoIO serviceInfoIO { get; set; } = new ServiceInfoIO();
        private static CancellationToken StoppingToken { get; set; }

        public static Task ExecuteServiceAsync(ILogger<Worker> logger, CancellationToken stoppingToken)
        {
            UpdateWatch.Start();
            return Task.Run(async () =>
            {
                Logger = logger;
                StoppingToken = stoppingToken;
                while (!stoppingToken.IsCancellationRequested)
                {
                    var sInfo = serviceInfoIO.GetServiceInfo();
                    if (File.Exists(LogPath))
                        ZipOldLog();

                    Log($"CycleInterval: {sInfo.CycleIntervalInHours}");
                    Log($"Cycle started.");
                    await StartDownloadCycle(sInfo);
                    Log("Cycle finished.");
                    Log($"Next cycle starts at {DateTime.Now.AddHours(sInfo.CycleIntervalInHours)}");

                    GC.Collect(3);
                    await Task.Delay(TimeSpan.FromHours(sInfo.CycleIntervalInHours), stoppingToken);

                    //await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
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

            var replayProvider = new ReplayProvider(serviceInfo.TokenInfo, new ReplayCache(), Logger);
            replayProvider.DownloadProgressUpdated += DownloadProgressUpdated;
            var advancedReplayProvider = new AdvancedReplayProvider(serviceInfo.TokenInfo, null, Logger);
            advancedReplayProvider.DownloadProgressUpdated += DownloadProgressUpdated;
            var replayFileProvider = new ReplayFileProvider(serviceInfo.TokenInfo, Logger);
            replayFileProvider.DownloadProgressUpdated += DownloadProgressUpdated;

            foreach (var filter in serviceInfo.Filters)
            {
                try
                {
                    //Download replay data
                    Log($"Started collecting replays from \"{filter.FilterName}\"");
                    var response = await replayProvider.CollectReplaysAsync(filter, StoppingToken, checkCache: true);
                    Log($"Collected {response.Replays.Count()} replays from \"{filter.FilterName}\" in {response.ElapsedMilliseconds / 1000d} seconds.");

                    //Download advanced replay data
                    var watch = Stopwatch.StartNew();
                    var advancedReplays = await advancedReplayProvider.GetAdvancedReplayInfosAsync(new List<Replay>(response.Replays), singleThreaded: true);
                    Log($"Collected {advancedReplays.Count} advanced replays from filter \"{filter.FilterName}\" in {watch.ElapsedMilliseconds / 1000d} seconds.");


                    //Download replay files
                    if (filter.AlsoSaveReplayFiles)
                    {
                        watch = Stopwatch.StartNew();
                        var nameIdPairs = new List<(string name, string id)>();
                        foreach (var replay in response.Replays)
                            nameIdPairs.Add((replay.RocketLeagueId, replay.Id));

                        await replayFileProvider.DownloadAndSaveReplayFilesAsync(filter.ReplayFilePath, nameIdPairs, StoppingToken);
                        Log($"Collected {advancedReplays.Count} replay files from filter \"{filter.FilterName}\" in {watch.ElapsedMilliseconds / 1000d} seconds.");
                    }
                    //Attempt to clear RAM
                    advancedReplays.Clear();
                    response = null;
                    advancedReplays = null;
                    GC.Collect(3);
                }
                catch (Exception ex)
                {
                    Log($"There was an error. Aborted download of this filter. Error message: {ex.Message}. Error type: {ex.GetType()}");
                }
            }
            Log(" ");
            LogCalls(replayFileProvider.GetAndDeleteApiCalls());

            replayProvider.DownloadProgressUpdated -= DownloadProgressUpdated;
            advancedReplayProvider.DownloadProgressUpdated -= DownloadProgressUpdated;
            replayFileProvider.DownloadProgressUpdated -= DownloadProgressUpdated;
        }

        private static void DownloadProgressUpdated(object sender, ProgressState e)
        {
            var progressBar = (!e.TotalCount.Equals(0)) ? GetProgressBar((double)e.PartCount / (double)(e.TotalCount - e.FalsePartCount)) : string.Empty;
            var outOf = $"{e.PartCount}/{e.TotalCount - e.FalsePartCount}";
            var text = $"{e.CurrentMessage} {outOf} {progressBar}";
            if (UpdateWatch.ElapsedMilliseconds > 200 || e.LastCall)
            {
                UpdateWatch.Restart();
                Log(text, true);
            }
        }

        private static void Log(string text, bool debug = false)
        {
            text = $"{DateTime.Now}: {text}";
            if (!debug)
                Logger.LogInformation(text);
            File.AppendAllLines(LogPath, new string[] { text });
        }

        private static void LogCalls(string[] calls)
        {
            Log($"Calls this session: {calls.Length}");
            File.AppendAllLines(LogPath, calls);
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

        private static void ZipOldLog()
        {
            ZipFile archive;
            if (!File.Exists(ServiceLogsZipPath))
                archive = new ZipFile(ServiceLogsZipPath);
            else
                archive = ZipFile.Read(ServiceLogsZipPath);
            archive.AddEntry($"{DateTime.Now:yyyy-MM-dd hh-mm-ss}.txt", File.ReadAllText(LogPath));
            archive.Save();
            File.Delete(LogPath);
        }

        //private static void AppendLogLine(string text)
        //{
        //    Logger.LogInformation(text);
        //    File.AppendAllLines(LogPath, new string[] { text });
        //}

        //private static void UpdateLastLogLine(string text)
        //{
        //    lock (LogPath)
        //    {
        //        var lines = File.ReadAllLines(LogPath);
        //        lines[lines.Length - 1] = text;
        //        File.WriteAllLines(LogPath, lines);
        //    }
        //}
    }
}