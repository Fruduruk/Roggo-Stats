using RLStats_Classes.Models;

namespace RLStats_Classes.MainClasses
{
    public class ReplayAssembler
    {
        public dynamic JData { get; }

        public ReplayAssembler(dynamic jData)
        {
            JData = jData;
        }

        public Replay Assemble()
        {
            var replay = new Replay
            {
                Id = JData.id,
                RocketLeagueId = JData.rocket_league_id,
                SeasonType = JData.season_type ?? "before Free2Play",
                Visibility = JData.visibility,
                Link = JData.link,
                Title = JData.replay_title,
                Playlist = JData.playlist_id,
                Season = JData.season,
                Date = JData.date,
                Uploader = JData.uploader.name,
                Blue = GetTeam(JData.blue),
                Orange = GetTeam(JData.orange)
            };
            return replay;
        }

        private Team GetTeam(dynamic r)
        {
            var t = new Team();
            if (r.goals != null)
                t.Goals = r.goals;
            if (r.players != null)
                foreach (var p in r.players)
                {
                    var player = new Player
                    {
                        Name = p.name,
                        MVP = p.mvp != null && (bool)p.mvp,
                        Score = p.score != null ? (int)p.score : 0,
                        StartTime = p.start_time,
                        EndTime = p.end_time
                    };
                    t.Players.Add(player);
                }
            return t;
        }
    }
}
