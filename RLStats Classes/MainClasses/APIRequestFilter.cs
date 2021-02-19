using RLStats_Classes.Enums;
using System;
using System.Collections.Generic;

namespace RLStats_Classes.MainClasses
{
    public class APIRequestFilter
    {
        public string FilterName { get; set; }
        #region Checks
        public bool CheckName { get; set; }
        public bool CheckTitle { get; set; }
        public bool CheckPlaylist { get; set; }
        public bool CheckSeason { get; set; }
        public bool CheckMatchResult { get; set; }
        public bool CheckSteamID { get; set; }
        public bool CheckDate { get; set; }
        #endregion
        #region Properties
        public List<string> Names { get; set; }
        public string Title { get; set; }
        public Playlist Playlist { get; set; }
        public bool FreeToPlaySeason { get; set; }
        public int Season { get; set; }
        public MatchResult MatchResult { get; set; }
        public bool Pro { get; set; }
        public List<string> SteamIDs { get; set; }
        public Tuple<DateTime, DateTime> DateRange { get; set; }
        #endregion

        public APIRequestFilter()
        {
            SetDefaultValues();
        }

        private void SetDefaultValues()
        {
            FilterName = string.Empty;

            CheckName = false;
            CheckTitle = false;
            CheckPlaylist = false;
            CheckSeason = false;
            CheckMatchResult = false;
            CheckSteamID = false;
            CheckDate = false;

            Names = new List<string>();
            Title = string.Empty;
            Playlist = 0;
            FreeToPlaySeason = true;
            Season = RLConstants.CurrentSeason;
            MatchResult = 0;
            Pro = false;
            SteamIDs = new List<string>();
            DateRange = new Tuple<DateTime, DateTime>(DateTime.Today, DateTime.Today);
        }

        public string GetApiUrl()
        {
            var builder = new APIRequestBuilder();
            if (CheckName)
                builder.SetPlayerNames(Names);
            if (CheckTitle)
                builder.SetTitle(Title);
            if (CheckPlaylist)
                builder.SetPlaylist(Playlist);
            if (CheckSeason)
                builder.SetSeason(Season);
            if (CheckMatchResult)
                builder.SetMatchResult(MatchResult);
            if (CheckSteamID)
                builder.SetSteamIds(SteamIDs);
            if(Pro)
                builder.SetPro(Pro);
            if (CheckDate)
            {
                //Ballchasing doesn't support date queries yet
            }
            return builder.GetApiUrl();
        }

        public void Overwrite(APIRequestFilter f)
        {
            this.FilterName = f.FilterName;

            this.CheckName = f.CheckName;
            this.CheckTitle = f.CheckTitle;
            this.CheckPlaylist = f.CheckPlaylist;
            this.CheckSeason = f.CheckSeason;
            this.CheckMatchResult = f.CheckMatchResult;
            this.CheckSteamID = f.CheckSteamID;
            this.CheckDate = f.CheckDate;

            this.Names = f.Names;
            this.Title = f.Title;
            this.Playlist = f.Playlist;
            this.FreeToPlaySeason = f.FreeToPlaySeason;
            this.Season = f.Season;
            this.MatchResult = f.MatchResult;
            this.Pro = f.Pro;
            this.SteamIDs = f.SteamIDs;
            this.DateRange = f.DateRange;
        }
    }
}
