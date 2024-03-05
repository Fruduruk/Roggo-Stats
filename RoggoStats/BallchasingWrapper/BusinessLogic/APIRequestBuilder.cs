using BallchasingWrapper.Models.Enums;

namespace BallchasingWrapper.BusinessLogic
{
    // ReSharper disable once InconsistentNaming
    public class APIRequestBuilder
    {
        public const string BallchasingApiUrl = "https://ballchasing.com/api/replays";
        private bool _anyAdded = false;
        private string Base { get; set; } = BallchasingApiUrl;

        public static string GetReplayFileUrl(string replayId)
        {
            return $"{BallchasingApiUrl}/{replayId}/file";
        }

        public void SetPlayerName(string value)
        {
            AddBinding();
            Base += "player-name=" + value;
            _anyAdded = true;
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
            _anyAdded = true;
        }
        public void SetPlaylist(Playlist value)
        {
            AddBinding();
            Base += "playlist=" + ConvertPlaylist(value);
            _anyAdded = true;
        }
        public void SetSeason(int value)
        {
            AddBinding();
            Base += "season=" + value.ToString();
            _anyAdded = true;
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
            _anyAdded = true;
        }
        public void SetSteamId(string value)
        {
            AddBinding();
            Base += "player-id=steam:" + value;
            _anyAdded = true;
        }
        public void SetSteamIds(List<string> steamIDs)
        {
            foreach (var id in steamIDs)
                SetSteamId(id);
        }
        public void SetPro(bool value)
        {
            AddBinding();
            if (value)
                Base += "pro=true";
            else
                Base += "pro=false";
            _anyAdded = true;
        }
        public void SetStartDate(DateTime startDate)
        {
            AddBinding();
            Base += "replay-date-after=" + startDate.ToRfc3339String();
            _anyAdded = true;
        }
        public void SetEndDate(DateTime endDate)
        {
            AddBinding();
            Base += "replay-date-before=" + endDate.ToRfc3339String();
            _anyAdded = true;
        }
        public void SetSortByUploadDate()
        {
            AddBinding();
            Base += "sort-by=created";
            _anyAdded = true;
        }
        public string GetApiUrl()
        {
            return Base;
        }
        public void Clear()
        {
            Base = BallchasingApiUrl;
            _anyAdded = false;
        }

        private void AddBinding() => Base += _anyAdded ? "&" : "?";

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
