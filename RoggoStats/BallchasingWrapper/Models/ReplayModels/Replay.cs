namespace BallchasingWrapper.Models.ReplayModels
{
    public class Replay
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("link")]
        public string? Link { get; set; }

        [JsonProperty("rocket_league_id")]
        public string? RocketLeagueId { get; set; }

        [JsonProperty("replay_title")]
        public string? Title { get; set; }

        [JsonProperty("visibility")]
        public string? Visibility { get; set; }

        [JsonProperty("map_code")]
        public string? MapCode { get; set; }

        [JsonProperty("playlist_id")]
        public string? Playlist { get; set; }

        [JsonProperty("playlist_name")]
        public string? PlaylistName { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("overtime")]
        public bool Overtime { get; set; }

        [JsonProperty("season")]
        public int Season { get; set; }

        [JsonProperty("season_type")]
        public string? SeasonType { get; set; } = "before Free2Play";

        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("date_has_tz")]
        public bool DateHasTimeZone { get; set; }

        [JsonProperty("min_rank")]
        public Rank? MinRank { get; set; }

        [JsonProperty("max_rank")]
        public Rank? MaxRank { get; set; }

        [JsonProperty("uploader")]
        public Uploader? Uploader { get; set; }

        [JsonProperty("blue")]
        public Team? TeamBlue { get; set; }

        [JsonProperty("orange")]
        public Team? TeamOrange { get; set; }

        // override object.Equals
        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            var that = obj as Replay;
            if (that != null && that.Id.Equals(Id))
                return true;
            if (!CheckEquality(SeasonType, that?.SeasonType))
                return false;
            if (!Season.Equals(that?.Season))
                return false;
            if (!CheckEquality(Playlist, that.Playlist))
                return false;
            return CheckEquality(TeamBlue, that.TeamBlue) &&
                   CheckEquality(TeamOrange, that.TeamOrange);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(SeasonType);
            hashCode.Add(Playlist);
            hashCode.Add(Season);
            hashCode.Add(TeamBlue);
            hashCode.Add(TeamOrange);
            var hash = hashCode.ToHashCode();
            return hash;
        }

        private static bool CheckEquality(object? ob1, object? ob2)
        {
            if (ob1 is null)
            {
                if (ob2 is null)
                    return true;
            }
            else
                if (ob1.Equals(ob2))
                return true;
            return false;
        }

        public bool HasNameInIt(string name)
        {
            return TeamOrange != null && TeamBlue != null &&
                   (TeamBlue.HasName(name) || TeamOrange.HasName(name));
        }

        public bool PlayedTogether(IEnumerable<Grpc.Identity> identities)
        {
            return identities.All(identity => identity.IdentityType switch
            {
                Grpc.IdentityType.Name => HasNameInIt(identity.NameOrId),
                Grpc.IdentityType.SteamId => HasIdInIt(identity.NameOrId),
                Grpc.IdentityType.EpicId => HasIdInIt(identity.NameOrId),
                Grpc.IdentityType.Ps4GamerTag => HasIdInIt(identity.NameOrId),
                _ => throw new ArgumentOutOfRangeException()
            } );
        }

        private bool HasIdInIt(string id)
        {
            return TeamOrange != null && TeamBlue != null &&
                   (TeamBlue.HasId(id) || TeamOrange.HasId(id));
        }
    }
}
