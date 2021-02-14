using RLStats_Classes.Enums;
using System;
using System.Collections.Generic;

namespace RLStats_Classes.MainClasses
{
    public class APIRequestBuilder
    {
        public const string BallchasingApiUrl = "https://ballchasing.com/api/replays";
        private bool anyAdded = false;
        private string Base { get; set; } = BallchasingApiUrl;
        public void SetPlayerName(string value)
        {
            AddBinding();
            Base += "player-name=" + value;
            anyAdded = true;
        }
        public void SetPlayerNames(List<string> names)
        {
            foreach (var name in names)
                SetPlayerName(name);
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
        public void SetSteamIDs(List<string> steamIDs)
        {
            foreach (var id in steamIDs)
                SetSteamID(id);
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
        public void SetStartDate(DateTime startDate)
        {
            //Doesn't work on Ballchasing.com
            //AddBinding();
            //Base += "replay-date-after=" + DateTimeHelper.ToRfc3339String(startDate);
            //anyAdded = true;
        }
        public void SetEndDate(DateTime endDate)
        {
            //Doesn't work on Ballchasing.com
            //AddBinding();
            //Base += "replay-date-before=" + DateTimeHelper.ToRfc3339String(endDate);
            //anyAdded = true;
        }
        public string GetApiUrl()
        {
            return Base;
        }
        public void Clear()
        {
            Base = BallchasingApiUrl;
            anyAdded = false;
        }
        private void AddBinding() => Base += (anyAdded ? "&" : "?");
        public static string GetSpecificReplayUrl(string id)
        {
            return $"{BallchasingApiUrl}/{id}";
        }
        private static string ConvertPlaylist(Playlist value)
        {
            var playlist = value switch
            {
                Playlist.UnrankedDuels => "unranked-duels",
                Playlist.UnrankedDoubles => "unranked-doubles",
                Playlist.UnrankedStandard => "unranked-standard",
                Playlist.UnrankedChaos => "unranked-chaos",
                Playlist.PrivateGame => "private",
                Playlist.Season => "season",
                Playlist.Offline => "offline",
                Playlist.RankedDuels => "ranked-duels",
                Playlist.RankedDoubles => "ranked-doubles",
                Playlist.RankedSoloStandard => "ranked-solo-standard",
                Playlist.RankedStandard => "ranked-standard",
                Playlist.Snowday => "snowday",
                Playlist.Rocketlabs => "rocketlabs",
                Playlist.Hoops => "hoops",
                Playlist.Rumble => "rumble",
                Playlist.Tournament => "tournament",
                Playlist.Dropshot => "dropshot",
                Playlist.RankedHoops => "ranked-hoops",
                Playlist.RankedRumble => "ranked-rumble",
                Playlist.RankedDropshot => "ranked-dropshot",
                Playlist.RankedSnowday => "ranked-snowday",
                Playlist.DropshotRumble => "dropshot-rumble",
                Playlist.Heatseeker => "heatseeker",
                _ => string.Empty
            };
            return playlist;
        }
    }
}
