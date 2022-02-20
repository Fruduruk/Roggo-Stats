
using System;

namespace RLStatsClasses.Models.ReplayModels.Advanced
{
    public class AdvancedReplay : IEquatable<AdvancedReplay>
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("uploader")]
        public Uploader Uploader { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("rocket_league_id")]
        public string RocketLeagueId { get; set; }

        [JsonProperty("match_guid")]
        public string MatchGuid { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("map_code")]
        public string MapCode { get; set; }

        [JsonProperty("match_type")]
        public string MatchType { get; set; }

        [JsonProperty("team_size")]
        public int? TeamSize { get; set; }

        [JsonProperty("playlist_id")]
        public string PlaylistId { get; set; }

        [JsonProperty("duration")]
        public int? Duration { get; set; }

        [JsonProperty("overtime")]
        public bool? Overtime { get; set; }

        [JsonProperty("overtime_seconds")]
        public int? OvertimeSeconds { get; set; }

        [JsonProperty("season")]
        public int? Season { get; set; }

        [JsonProperty("season_type")]
        public string SeasonType { get; set; } = "before Free2Play";

        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("date_has_timezone")]
        public bool? DateHasTimezone { get; set; }

        [JsonProperty("visibility")]
        public string Visibility { get; set; }

        [JsonProperty("min_rank")]
        public Rank MinRank { get; set; }

        [JsonProperty("max_rank")]
        public Rank MaxRank { get; set; }

        [JsonProperty("blue")]
        public AdvancedTeam TeamBlue { get; set; }

        [JsonProperty("orange")]
        public AdvancedTeam TeamOrange { get; set; }

        [JsonProperty("playlist_name")]
        public string PlaylistName { get; set; }

        [JsonProperty("map_name")]
        public string MapName { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as AdvancedReplay);
        }

        public bool Equals(AdvancedReplay other)
        {
            if (other is null)
                return false;
            return other.Id == Id;
        }

        public override int GetHashCode()
        {
            if (Id != null)
                return Id.GetHashCode();
            return 0;
        }

        public bool Contains(string nameOrSteamId)
        {
            if (TeamBlue != null)
                if (TeamBlue.Players != null)
                    foreach (var p in TeamBlue.Players)
                    {
                        if (p.Name != null)
                            if (p.Name.ToLower().Equals(nameOrSteamId.ToLower()))
                                return true;
                        if (p.Id != null)
                            if (p.Id.Id != null)
                                if (p.Id.Id.ToLower().Equals(nameOrSteamId.ToLower()))
                                    return true;
                    }
            if (TeamOrange != null)
                if (TeamOrange.Players != null)
                    foreach (var p in TeamOrange.Players)
                    {
                        if (p.Name != null)
                            if (p.Name.ToLower().Equals(nameOrSteamId.ToLower()))
                                return true;
                        if (p.Id != null)
                            if (p.Id.Id != null)
                                if (p.Id.Id.ToLower().Equals(nameOrSteamId.ToLower()))
                                    return true;
                    }
            return false;
        }

        public PlayerStats GetPlayerStats(string nameOrSteamId)
        {
            if (TeamBlue != null)
                if (TeamBlue.Players != null)
                    foreach (var p in TeamBlue.Players)
                    {
                        if (p.Name != null)
                            if (p.Name.ToLower().Equals(nameOrSteamId.ToLower()))
                                return p.Stats;
                        if (p.Id != null)
                            if (p.Id.Id != null)
                                if (p.Id.Id.ToLower().Equals(nameOrSteamId.ToLower()))
                                    return p.Stats;
                    }
            if (TeamOrange != null)
                if (TeamOrange.Players != null)
                    foreach (var p in TeamOrange.Players)
                    {
                        if (p.Name != null)
                            if (p.Name.ToLower().Equals(nameOrSteamId.ToLower()))
                                return p.Stats;
                        if (p.Id != null)
                            if (p.Id.Id != null)
                                if (p.Id.Id.ToLower().Equals(nameOrSteamId.ToLower()))
                                    return p.Stats;
                    }
            return null;
        }
    }
}
