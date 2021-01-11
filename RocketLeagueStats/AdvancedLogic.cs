﻿using RocketLeagueStats.AdvancedModels;
using RocketLeagueStats.AverageModels;
using RocketLeagueStats.ChartModels;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RocketLeagueStats
{
    public class AdvancedLogic
    {
        public List<WinratePack> CalculateWeekDayWinrates(List<AdvancedReplay> replays, string nameOrId)
        {
            var weekWinrates = new List<WinratePack>();
            foreach (var weekDay in Enum.GetValues(typeof(DayOfWeek)))
            {
                List<AdvancedReplay> commonWeekDayReplays = new List<AdvancedReplay>();
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
            List<AdvancedReplay> goodReplays = GetAllReplaysWithMapName(replays);
            List<string> names = GetMapNames(goodReplays);
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
                string playersTeam = GetPlayersTeamColor(nameOrId, r);
                if (!playersTeam.Equals(string.Empty))
                {
                    played = played + 1;
                    string teamThatWon = string.Empty;
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

        private string GetPlayersTeamColor(string nameOrID, AdvancedReplay r)
        {
            string playersTeam = string.Empty;
            try
            {
                if (r.Blue != null)
                    if (r.Blue.Players != null)
                        foreach (var p in r.Blue.Players)
                        {
                            if (p.Name != null)
                                if (p.Name.Equals(nameOrID))
                                    playersTeam = r.Blue.Color;
                            if (p.Id.ID != null)
                                if (p.Id.Platform.Equals("steam"))
                                    if (p.Id.ID.Equals(nameOrID))
                                        playersTeam = r.Orange.Color;
                        }
                if (playersTeam != null)
                    if (r.Orange != null)
                        if (r.Orange.Players != null)
                            foreach (var p in r.Orange.Players)
                            {
                                if (p.Name != null)
                                    if (p.Name.Equals(nameOrID))
                                        playersTeam = r.Orange.Color;
                                if (p.Id.ID != null)
                                    if (p.Id.Platform.Equals("steam"))
                                        if (p.Id.ID.Equals(nameOrID))
                                            playersTeam = r.Orange.Color;
                            }

            }
            catch
            {
                playersTeam = string.Empty;
            }
            return playersTeam;
        }

        public async Task<Dictionary<string,AveragePlayerStats>> GetAveragesAsync(List<AdvancedReplay> advancedReplays, List<string> names)
        {
            Dictionary<string, AveragePlayerStats> allAveragePlayerStats = new Dictionary<string, AveragePlayerStats>();
            foreach (var name in names)
            {
                CalculateAveragesAndAddToListAsync(advancedReplays, allAveragePlayerStats, name);
            }
            await Task.Run(() =>
            {
                while (allAveragePlayerStats.Count != names.Count)
                {
                    Thread.Sleep(10);
                }
            });
            return allAveragePlayerStats;
        }

        private async void CalculateAveragesAndAddToListAsync(List<AdvancedReplay> advancedReplays, Dictionary<string, AveragePlayerStats> allAveragePlayerStats,  string name)
        {
            AveragePlayerStats averageStatsForOnePlayer = await GetAverageStatsForOnePlayer(advancedReplays, name);
            lock (allAveragePlayerStats)
            {
                allAveragePlayerStats.Add(name,averageStatsForOnePlayer);
            }
        }

        public async Task<AveragePlayerStats> GetAverageStatsForOnePlayer(List<AdvancedReplay> advancedReplays, string name)
        {
            List<PlayerStats> allStatsForOnePlayer = new List<PlayerStats>();
            AveragePlayerStats averageStatsForOnePlayer = new AveragePlayerStats();
            await Task.Run(() =>
            {
                foreach (var replay in advancedReplays)
                {
                    if (replay.Contains(name))
                    {
                        PlayerStats playerStats = replay.GetPlayerStats(name);
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
            List<string> names = new List<string>();
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
            List<AdvancedReplay> goodReplays = new List<AdvancedReplay>();
            foreach (var r in replays)
                if (r.Map_name != null)
                    goodReplays.Add(r);
            return goodReplays;
        }
    }
}