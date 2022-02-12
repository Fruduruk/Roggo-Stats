
using System;

namespace RLStats_Classes.Models.Advanced
{
    public class AdvancedReplay : IEquatable<AdvancedReplay>
    {
        public string Id { get; set; }
        public string Link { get; set; }
        public DateTime Created { get; set; }
        public Uploader Uploader { get; set; }
        public string Status { get; set; }
        public string Rocket_league_id { get; set; }
        public string Match_guid { get; set; }
        public string Title { get; set; }
        public string Map_code { get; set; }
        public string Match_type { get; set; }
        public int? Team_size { get; set; }
        public string Playlist_id { get; set; }
        public int? Duration { get; set; }
        public bool? Overtime { get; set; }
        public int? Overtime_seconds { get; set; }
        public int? Season { get; set; }
        public string Season_type { get; set; }
        public DateTime Date { get; set; }
        public string Visibility { get; set; }
        public AdvancedTeam Blue { get; set; }
        public AdvancedTeam Orange { get; set; }
        public string Playlist_name { get; set; }
        public string Map_name { get; set; }

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
            if (Blue != null)
                if (Blue.Players != null)
                    foreach (var p in Blue.Players)
                    {
                        if (p.Name != null)
                            if (p.Name.ToLower().Equals(nameOrSteamId.ToLower()))
                                return true;
                        if (p.Id != null)
                            if (p.Id.ID != null)
                                if (p.Id.ID.ToLower().Equals(nameOrSteamId.ToLower()))
                                    return true;
                    }
            if (Orange != null)
                if (Orange.Players != null)
                    foreach (var p in Orange.Players)
                    {
                        if (p.Name != null)
                            if (p.Name.ToLower().Equals(nameOrSteamId.ToLower()))
                                return true;
                        if (p.Id != null)
                            if (p.Id.ID != null)
                                if (p.Id.ID.ToLower().Equals(nameOrSteamId.ToLower()))
                                    return true;
                    }
            return false;
        }

        public PlayerStats GetPlayerStats(string nameOrSteamId)
        {
            if (Blue != null)
                if (Blue.Players != null)
                    foreach (var p in Blue.Players)
                    {
                        if (p.Name != null)
                            if (p.Name.ToLower().Equals(nameOrSteamId.ToLower()))
                                return p.Stats;
                        if (p.Id != null)
                            if (p.Id.ID != null)
                                if (p.Id.ID.ToLower().Equals(nameOrSteamId.ToLower()))
                                    return p.Stats;
                    }
            if (Orange != null)
                if (Orange.Players != null)
                    foreach (var p in Orange.Players)
                    {
                        if (p.Name != null)
                            if (p.Name.ToLower().Equals(nameOrSteamId.ToLower()))
                                return p.Stats;
                        if (p.Id != null)
                            if (p.Id.ID != null)
                                if (p.Id.ID.ToLower().Equals(nameOrSteamId.ToLower()))
                                    return p.Stats;
                    }
            return null;
        }
    }
}
