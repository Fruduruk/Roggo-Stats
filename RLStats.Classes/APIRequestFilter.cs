using RLStats_Classes.Enums;

using System;
using System.Collections.Generic;

namespace RLStats_Classes
{
    public class APIRequestFilter
    {
        public int ReplayCap { get; set; } = 0;
        public string FilterName { get; set; }

        #region Checks
        public bool AlsoSaveReplayFiles { get; set; }
        public bool CheckName { get; set; }
        public bool CheckTitle { get; set; }
        public bool CheckPlaylist { get; set; }
        public bool CheckSeason { get; set; }
        public bool CheckMatchResult { get; set; }
        public bool CheckSteamId { get; set; }
        public bool CheckDate { get; set; }
        #endregion
        #region Properties
        public string ReplayFilePath { get; set; } = string.Empty;
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
            ReplayCap = 0;
            FilterName = string.Empty;

            AlsoSaveReplayFiles = false;
            CheckName = false;
            CheckTitle = false;
            CheckPlaylist = false;
            CheckSeason = false;
            CheckMatchResult = false;
            CheckSteamId = false;
            CheckDate = false;

            ReplayFilePath = string.Empty;
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
            if (CheckSteamId)
                builder.SetSteamIds(SteamIDs);
            if (Pro)
                builder.SetPro(Pro);
            if (CheckDate)
            {
                builder.SetStartDate(DateRange.Item1);
                builder.SetEndDate(DateRange.Item2.AddDays(1));
            }

            //Always sort them by upload date
            builder.SetSortByUploadDate();

            return builder.GetApiUrl();
        }
    }
}
