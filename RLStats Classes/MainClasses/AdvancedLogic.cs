using RLStats_Classes.AdvancedModels;
using RLStats_Classes.AverageModels;
using RLStats_Classes.ChartModels;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RLStats_Classes.MainClasses.Interfaces;

namespace RLStats_Classes.MainClasses
{
    public class AdvancedLogic : IStatsComparer
    {
        public List<WinratePack> CalculateWeekDayWinrates(List<AdvancedReplay> replays, string nameOrId)
        {
            var weekWinrates = new List<WinratePack>();
            foreach (var weekDay in Enum.GetValues(typeof(DayOfWeek)))
            {
                var commonWeekDayReplays = new List<AdvancedReplay>();
                foreach (var r in replays)
                    if (r.Date.DayOfWeek.Equals(weekDay))
                        commonWeekDayReplays.Add(r);
                weekWinrates.Add(GetWinratePack(nameOrId, weekDay.ToString(), commonWeekDayReplays));
            }
            return weekWinrates;
        }

        public List<WinratePack> CalculateMapWinRates
            (List<AdvancedReplay> replays, string nameOrId)
        {
            var mapWinrates = new List<WinratePack>();
            var goodReplays = GetAllReplaysWithMapName(replays);
            var names = GetMapNames(goodReplays);
            foreach (var s in names)
            {
                List<AdvancedReplay> commonMapReplays = new List<AdvancedReplay>();
                foreach (var r in goodReplays)
                    if (r.Map_name.Equals(s))
                        commonMapReplays.Add(r);
                mapWinrates.Add(GetWinratePack(nameOrId, s, commonMapReplays));
            }
            return mapWinrates;
        }

        private WinratePack GetWinratePack(string nameOrId, string winratePackName, List<AdvancedReplay> commonMapReplays)
        {
            var wp = new WinratePack();
            double played = 0;
            double won = 0;
            foreach (var r in commonMapReplays)
            {
                var playersTeam = GetPlayersTeamColor(nameOrId, r);
                if (!playersTeam.Equals(string.Empty))
                {
                    played = played + 1;
                    var teamThatWon = string.Empty;
                    if (r.Orange.Stats.Core.Goals != r.Blue.Stats.Core.Goals)
                    {
                        if (r.Orange.Stats.Core.Goals > r.Blue.Stats.Core.Goals)
                            teamThatWon = r.Orange.Color;
                        else
                            teamThatWon = r.Blue.Color;
                        if (teamThatWon.Equals(playersTeam))
                            won = won + 1;
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
                                        playersTeam = r.Orange.Color;
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

        public async Task<Dictionary<string, AveragePlayerStats>> GetAveragesAsync(IEnumerable<AdvancedReplay> advancedReplays, IEnumerable<string> names)
        {
            var nameList = new List<string>(names);
            var replays = new List<AdvancedReplay>(advancedReplays);
            var allAveragePlayerStats = new Dictionary<string, AveragePlayerStats>();
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

        private async Task CalculateAveragesAndAddToListAsync(List<AdvancedReplay> advancedReplays, Dictionary<string, AveragePlayerStats> allAveragePlayerStats, string name)
        {
            var averageStatsForOnePlayer = await GetAverageStatsForOnePlayer(advancedReplays, name);
            lock (allAveragePlayerStats)
            {
                allAveragePlayerStats.Add(name, averageStatsForOnePlayer);
            }
        }

        public async Task<AveragePlayerStats> GetAverageStatsForOnePlayer(List<AdvancedReplay> advancedReplays, string name)
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
                averageStatsForOnePlayer = AveragePlayerStats.GetAveragePlayerStats(allStatsForOnePlayer);
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