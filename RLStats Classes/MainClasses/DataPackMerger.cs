using RLStats_Classes.Models;
using System;
using System.Collections.Generic;

namespace RLStats_Classes.MainClasses
{
    public class DataPackMerger
    {
        public static List<Replay> GetReplaysForTimeZone(List<Replay> replays, DateTime start, DateTime end)
        {
            var replaysInTimeZone = new List<Replay>();
            foreach(var r in replays)
            {
                if (r.Date > start)
                    if (r.Date < end)
                        replaysInTimeZone.Add(r);
            }
            return replaysInTimeZone;
        }

        public static bool DoesListContainReplay(List<Replay> replays, Replay r)
        {
            foreach (var replay in replays)
                if (r.Equals(replay))
                    return true;
            return false;
        }
    }
}
