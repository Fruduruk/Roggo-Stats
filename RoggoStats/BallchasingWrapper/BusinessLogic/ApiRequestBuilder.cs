namespace BallchasingWrapper.BusinessLogic
{
    public class ApiRequestBuilder
    {
        private const string BallchasingApiUrl = "https://ballchasing.com/api/replays";
        private bool _anyAdded = false;
        private string _base = BallchasingApiUrl;

        public static string GetReplayFileUrl(string replayId)
        {
            return $"{BallchasingApiUrl}/{replayId}/file";
        }

        public ApiRequestBuilder()
        {
        }

        public ApiRequestBuilder(ApiRequestBuilder clone)
        {
            _anyAdded = clone._anyAdded;
            _base = clone._base;
        }

        public void SetPlayerName(string value)
        {
            AddBinding();
            _base += "player-name=" + value;
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
            _base += "title=" + value;
            _anyAdded = true;
        }

        public void SetPlaylist(string value)
        {
            AddBinding();
            _base += "playlist=" + value;
            _anyAdded = true;
        }

        public void SetSeason(string value)
        {
            AddBinding();
            _base += "season=" + value;
            _anyAdded = true;
        }

        public void SetSteamId(string value)
        {
            AddBinding();
            _base += "player-id=steam:" + value;
            _anyAdded = true;
        }

        public void SetEpicId(string epicId)
        {
            AddBinding();
            //Not sure if "epic" is right.
            _base += "player-id=epic:" + epicId;
            _anyAdded = true;
        }

        public void SetPs4GamerTag(string gamerTag)
        {
            AddBinding();
            _base += "player-id=ps4:" + gamerTag;
            _anyAdded = true;
        }

        public void SetMinDate(string startDate)
        {
            AddBinding();
            _base += "replay-date-after=" + startDate;
            _anyAdded = true;
        }

        public void SetMaxDate(string endDate)
        {
            AddBinding();
            _base += "replay-date-before=" + endDate;
            _anyAdded = true;
        }

        public void SetMinRank(string rank)
        {
            AddBinding();
            _base += "min-rank=" + rank;
            _anyAdded = true;
        }
        
        public void SetMaxRank(string rank)
        {
            AddBinding();
            _base += "max-rank=" + rank;
            _anyAdded = true;
        }

        public void SetSortByUploadDate()
        {
            AddBinding();
            _base += "sort-by=created";
            _anyAdded = true;
        }

        public string GetApiUrl()
        {
            return _base;
        }

        public void Clear()
        {
            _base = BallchasingApiUrl;
            _anyAdded = false;
        }

        private void AddBinding() => _base += _anyAdded ? "&" : "?";

        public static string GetSpecificReplayUrl(string id)
        {
            return $"{BallchasingApiUrl}/{id}";
        }
    }
}