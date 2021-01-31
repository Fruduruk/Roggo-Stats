using RLStats_Classes.Enums;
using System;
using System.Collections.Generic;

namespace RLStats_Classes.MainClasses
{
    public class APIRequestFilter
    {
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
        public Tuple<DateTime,DateTime> DateRange { get; set; }
        #endregion
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
                builder.SetSteamIDs(SteamIDs);
            if(CheckDate)
            {
                //Ballchasing doesn't support date queries yet
            }
            return builder.GetApiUrl();
        }
    }
}
