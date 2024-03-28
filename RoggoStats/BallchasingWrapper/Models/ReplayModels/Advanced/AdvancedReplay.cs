namespace BallchasingWrapper.Models.ReplayModels.Advanced
{
    public class AdvancedReplay : IEquatable<AdvancedReplay>
    {
        [JsonProperty("id")] public string Id { get; set; } = string.Empty;
        [JsonProperty("link")] public string Link { get; set; } = string.Empty;
        [JsonProperty("created")] public DateTime Created { get; set; }
        [JsonProperty("uploader")] public Uploader Uploader { get; set; } = new();
        [JsonProperty("status")] public string Status { get; set; } = string.Empty;
        [JsonProperty("rocket_league_id")] public string RocketLeagueId { get; set; } = string.Empty;
        [JsonProperty("match_guid")] public string MatchGuid { get; set; } = string.Empty;
        [JsonProperty("title")] public string Title { get; set; } = string.Empty;
        [JsonProperty("map_code")] public string MapCode { get; set; } = string.Empty;
        [JsonProperty("match_type")] public string MatchType { get; set; } = string.Empty;
        [JsonProperty("team_size")] public int TeamSize { get; set; }
        [JsonProperty("playlist_id")] public string PlaylistId { get; set; } = string.Empty;
        [JsonProperty("duration")] public int Duration { get; set; }
        [JsonProperty("overtime")] public bool Overtime { get; set; }
        [JsonProperty("overtime_seconds")] public int OvertimeSeconds { get; set; }
        [JsonProperty("season")] public int Season { get; set; }
        [JsonProperty("season_type")] public string SeasonType { get; set; } = "before Free2Play";
        [JsonProperty("date")] public DateTime Date { get; set; }
        [JsonProperty("date_has_timezone")] public bool? DateHasTimezone { get; set; }
        [JsonProperty("visibility")] public string Visibility { get; set; } = string.Empty;
        [JsonProperty("min_rank")] public Rank? MinRank { get; set; } = new();
        [JsonProperty("max_rank")] public Rank? MaxRank { get; set; } = new();
        [JsonProperty("blue")] public AdvancedTeam TeamBlue { get; set; } = new();
        [JsonProperty("orange")] public AdvancedTeam TeamOrange { get; set; } = new();
        [JsonProperty("playlist_name")] public string PlaylistName { get; set; } = string.Empty;
        [JsonProperty("map_name")] public string MapName { get; set; } = string.Empty;
        [JsonProperty("server")] public Server? Server { get; set; }
        public override bool Equals(object obj)
        {
            return Equals(obj as AdvancedReplay);
        }

        public bool Equals(AdvancedReplay? other)
        {
            if (other is null)
                return false;
            return other.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id != null ? Id.GetHashCode() : 0;
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