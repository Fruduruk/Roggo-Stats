﻿using System;

namespace RocketLeagueStats
{
    public class APIUrlBuilder
    {
        public const string BallchasingApiUrl = "https://ballchasing.com/api/replays";
        private bool anyAdded = false;
        public bool CheckForDate { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }


        private string Base = BallchasingApiUrl;
        public static string GetSpecificReplayUrl(string id)
        {
            return $"{BallchasingApiUrl}/{id}";
        }
        public void SetStartDate(DateTime startDate)
        {
            StartDate = startDate;
            CheckForDate = true;
            //Doesn't work on Ballchasing.com
            //AddBinding();
            //Base += "replay-date-after=" + DateTimeHelper.ToRfc3339String(startDate);
            //anyAdded = true;
        }
        public void SetEndDate(DateTime endDate)
        {
            EndDate = endDate;
            CheckForDate = true;
            //Doesn't work on Ballchasing.com
            //AddBinding();
            //Base += "replay-date-before=" + DateTimeHelper.ToRfc3339String(endDate);
            //anyAdded = true;
        }

        public void SetPlayerName(string value)
        {
            AddBinding();
            Base += "player-name=" + value;
            anyAdded = true;
        }
        public void SetTitle(string value)
        {
            AddBinding();
            Base += "title=" + value;
            anyAdded = true;
        }
        public void SetPlaylist(Playlist value)
        {
            AddBinding();
            Base += "playlist=" + ConvertPlaylist(value);
            anyAdded = true;
        }
        public void SetSeason(int value)
        {
            AddBinding();
            Base += "season=" + value.ToString();
            anyAdded = true;
        }
        public void SetMatchResult(MatchResult value)
        {
            AddBinding();
            switch (value)
            {
                case MatchResult.Win:
                    Base += "match-result=win";
                    break;
                case MatchResult.Loss:
                    Base += "match-result=loss";
                    break;
            }
            anyAdded = true;
        }
        public void SetSteamID(string value)
        {
            AddBinding();
            Base += "player-id=steam:" + value;
            anyAdded = true;
        }
        public void SetPro(bool value)
        {
            AddBinding();
            if (value)
                Base += "pro=true";
            else
                Base += "pro=false";
            anyAdded = true;
        }
        public string GetApiUrl()
        {
            return Base;
        }
        private void AddBinding() => Base += (anyAdded ? "&" : "?");
        private static string ConvertPlaylist(Playlist value)
        {
            string playlist = string.Empty;
            switch (value)
            {
                case Playlist.UnrankedDuels:
                    playlist = "unranked-duels";
                    break;
                case Playlist.UnrankedDoubles:
                    playlist = "unranked-doubles";
                    break;
                case Playlist.UnrankedStandard:
                    playlist = "unranked-standard";
                    break;
                case Playlist.UnrankedChaos:
                    playlist = "unranked-chaos";
                    break;
                case Playlist.PrivateGame:
                    playlist = "private";
                    break;
                case Playlist.Season:
                    playlist = "season";
                    break;
                case Playlist.Offline:
                    playlist = "offline";
                    break;
                case Playlist.RankedDuels:
                    playlist = "ranked-duels";
                    break;
                case Playlist.RankedDoubles:
                    playlist = "ranked-doubles";
                    break;
                case Playlist.RankedSoloStandard:
                    playlist = "ranked-solo-standard";
                    break;
                case Playlist.RankedStandard:
                    playlist = "ranked-standard";
                    break;
                case Playlist.Snowday:
                    playlist = "snowday";
                    break;
                case Playlist.Rocketlabs:
                    playlist = "rocketlabs";
                    break;
                case Playlist.Hoops:
                    playlist = "hoops";
                    break;
                case Playlist.Rumble:
                    playlist = "rumble";
                    break;
                case Playlist.Tournament:
                    playlist = "tournament";
                    break;
                case Playlist.Dropshot:
                    playlist = "dropshot";
                    break;
                case Playlist.RankedHoops:
                    playlist = "ranked-hoops";
                    break;
                case Playlist.RankedRumble:
                    playlist = "ranked-rumble";
                    break;
                case Playlist.RankedDropshot:
                    playlist = "ranked-dropshot";
                    break;
                case Playlist.RankedSnowday:
                    playlist = "ranked-snowday";
                    break;
                case Playlist.DropshotRumble:
                    playlist = "dropshot-rumble";
                    break;
                case Playlist.Heatseeker:
                    playlist = "heatseeker";
                    break;
            }
            return playlist;
        }

        public void Clear()
        {
            Base = BallchasingApiUrl;
            anyAdded = false;
        }
    }
}