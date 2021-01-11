using System;
using System.Collections.Generic;

namespace RocketLeagueStats
{
    public class ApiDataPack
    {
        public List<Replay> Replays { get; set; }
        public int ReplayCount { get; set; }
        public dynamic Next { get; set; }
        public bool Success { get; set; }
        public Exception Ex { get; set; }
        public string ReceivedString { get; set; } = string.Empty;

        public void DeleteObsoleteReplays()
        {
            List<Replay> newReplayList = new List<Replay>();
            foreach (var replay in Replays)
                if (!DataPackMerger.DoesListContainReplay(newReplayList, replay))
                    newReplayList.Add(replay);
            Replays = newReplayList;
        }
        public void DeleteReplaysWithoutSpecificNames(List<string> playerNames)
        {
            List<Replay> newReplays = new List<Replay>();

            foreach (var r in Replays)
            {
                int counter = 0;
                foreach (var name in playerNames)
                    if (r.HasNameInIt(name))
                        counter++;
                if(counter.Equals(playerNames.Count))
                    newReplays.Add(r);
            }

            Replays = newReplays;
        }
        public void DeleteReplaysThatAreNotInTimeRange(DateTime start, DateTime end)
        {
            Replays = DataPackMerger.GetReplaysForTimeZone(Replays, start, end);
        }
    }
}
