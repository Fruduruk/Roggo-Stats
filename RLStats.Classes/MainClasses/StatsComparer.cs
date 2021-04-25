using RLStats_Classes.AdvancedModels;
using RLStats_Classes.AverageModels;
using RLStats_Classes.ChartModels;
using RLStats_Classes.MainClasses.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace RLStats_Classes.MainClasses
{
    public class StatsComparer : IStatsComparer
    {
        public IEnumerable<WinratePack> CalculateWeekDayWinrates(IEnumerable<AdvancedReplay> advancedReplays, string nameOrSteamId)
        {
            if (advancedReplays is null)
                throw new NullReferenceException();
            if (string.IsNullOrEmpty(nameOrSteamId))
                throw new NullReferenceException();
            var weekWinrates = new List<WinratePack>();
            foreach (var weekDay in Enum.GetValues(typeof(DayOfWeek)))
            {
                var commonWeekDayReplays = new List<AdvancedReplay>();
                var replayArray = advancedReplays.ToArray();
                foreach (var r in replayArray)
                    if (r.Date.DayOfWeek.Equals(weekDay))
                        commonWeekDayReplays.Add(r);
                weekWinrates.Add(GetWinratePack(nameOrSteamId, weekDay.ToString(), commonWeekDayReplays));
            }
            return weekWinrates;
        }

        public IEnumerable<WinratePack> CalculateMapWinRates(IEnumerable<AdvancedReplay> advancedReplays, string nameOrSteamId)
        {
            var mapWinrates = new List<WinratePack>();
            var goodReplays = GetAllReplaysWithMapName(advancedReplays.ToList());
            var names = GetMapNames(goodReplays);
            foreach (var s in names)
            {
                var commonMapReplays = new List<AdvancedReplay>();
                foreach (var r in goodReplays)
                    if (r.Map_name.Equals(s))
                        commonMapReplays.Add(r);
                mapWinrates.Add(GetWinratePack(nameOrSteamId, s, commonMapReplays));
            }
            return mapWinrates;
        }

        private WinratePack GetWinratePack(string nameOrId, string winratePackName, List<AdvancedReplay> commonReplays)
        {
            var wp = new WinratePack();
            double played = 0;
            double won = 0;
            foreach (var r in commonReplays)
            {
                var playersTeam = GetPlayersTeamColor(nameOrId, r);
                if (!playersTeam.Equals(string.Empty))
                {
                    played++;
                    var teamThatWon = string.Empty;
                    if (r.Orange.Stats.Core.Goals != r.Blue.Stats.Core.Goals)
                    {
                        if (r.Orange.Stats.Core.Goals > r.Blue.Stats.Core.Goals)
                            teamThatWon = r.Orange.Color;
                        else
                            teamThatWon = r.Blue.Color;
                        if (teamThatWon.Equals(playersTeam))
                            won = 1 + won;
                    }
                }
            }
            wp.Name = winratePackName;
            wp.Won = won;
            wp.Played = played;
            return wp;
        }

        private string GetPlayersTeamColor(string nameOrId, AdvancedReplay r)
        {
            var playersTeam = string.Empty;
            try
            {
                if (r.Blue != null)
                    if (r.Blue.Players != null)
                        foreach (var p in r.Blue.Players)
                        {
                            if (p.Name != null)
                                if (p.Name.Equals(nameOrId))
                                    playersTeam = r.Blue.Color;
                            if (p.Id.ID != null)
                                if (p.Id.Platform.Equals("steam"))
                                    if (p.Id.ID.Equals(nameOrId))
                                        playersTeam = r.Blue.Color;
                        }
                if (playersTeam != null)
                    if (r.Orange != null)
                        if (r.Orange.Players != null)
                            foreach (var p in r.Orange.Players)
                            {
                                if (p.Name != null)
                                    if (p.Name.Equals(nameOrId))
                                        playersTeam = r.Orange.Color;
                                if (p.Id.ID != null)
                                    if (p.Id.Platform.Equals("steam"))
                                        if (p.Id.ID.Equals(nameOrId))
                                            playersTeam = r.Orange.Color;
                            }

            }
            catch
            {
                playersTeam = string.Empty;
            }
            return playersTeam;
        }

        public async Task<IEnumerable<AveragePlayerStats>> GetAveragesAsync(IEnumerable<AdvancedReplay> advancedReplays, IEnumerable<string> names)
        {
            var nameList = new List<string>(names);
            var replays = new List<AdvancedReplay>(advancedReplays);
            var allAveragePlayerStats = new List<AveragePlayerStats>();
            var taskList = new List<Task>();
            foreach (var name in nameList)
            {
                var task = CalculateAveragesAndAddToListAsync(replays, allAveragePlayerStats, name);
                taskList.Add(task);
            }
            await Task.Run(() =>
            {
                foreach(var task in taskList)
                    if (!task.IsCompleted)
                        task.Wait();
            });
            return allAveragePlayerStats;
        }
        
        private async Task CalculateAveragesAndAddToListAsync(IList<AdvancedReplay> advancedReplays, IList<AveragePlayerStats> allAveragePlayerStats, string name)
        {
            var averageStatsForOnePlayer = await GetAverageStatsForOnePlayer(advancedReplays, name);
            lock (allAveragePlayerStats)
            {
                allAveragePlayerStats.Add(averageStatsForOnePlayer);
            }
        }

        private async Task<AveragePlayerStats> GetAverageStatsForOnePlayer(IList<AdvancedReplay> advancedReplays, string name)
        {
            var allStatsForOnePlayer = new List<PlayerStats>();
            var averageStatsForOnePlayer = new AveragePlayerStats();
            await Task.Run(() =>
            {
                foreach (var replay in advancedReplays)
                {
                    if (replay.Contains(name))
                    {
                        var playerStats = replay.GetPlayerStats(name);
                        if (playerStats != null)
                            allStatsForOnePlayer.Add(playerStats);
                    }
                }
                averageStatsForOnePlayer = AveragePlayerStats.GetAveragePlayerStats(allStatsForOnePlayer, name);
            });
            return averageStatsForOnePlayer;
        }

        private List<string> GetMapNames(List<AdvancedReplay> legitReplays)
        {
            var names = new List<string>();
            foreach (var r in legitReplays)
            {
                var mapname = r.Map_name;
                if (!names.Contains(mapname))
                        names.Add(mapname);
            }

            return names;
        }
        private List<AdvancedReplay> GetAllReplaysWithMapName(List<AdvancedReplay> replays)
        {
            var goodReplays = new List<AdvancedReplay>();
            foreach (var r in replays)
                if (r.Map_name != null)
                    goodReplays.Add(r);
            return goodReplays;
        }
    }
}