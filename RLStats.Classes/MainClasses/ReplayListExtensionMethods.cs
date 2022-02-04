using RLStats_Classes.Models;

using System;
using System.Collections.Generic;

namespace RLStats_Classes.MainClasses
{
    public static class ReplayListExtensionMethods
    {
        public static List<Replay> GetReplaysInTimeZone(this IEnumerable<Replay> replays, DateTime start, DateTime end)
        {
            var replaysInTimeZone = new List<Replay>();
            foreach (var r in replays)
                if (r.Date > start)
                    if (r.Date < end)
                        replaysInTimeZone.Add(r);
            return replaysInTimeZone;
        }

        public static bool DoesListContainReplay(this IEnumerable<Replay> replays, Replay r)
        {
            foreach (var replay in replays)
                if (r.Equals(replay))
                    return true;
            return false;
        }

        public static int DeleteObsoleteReplays(this List<Replay> replays)
        {
            var newReplayList = new List<Replay>();
            foreach (var replay in replays)
                if (!newReplayList.DoesListContainReplay(replay))
                    newReplayList.Add(replay);
            var obsoleteCount = replays.Count - newReplayList.Count;
            replays.Clear();
            replays.AddRange(newReplayList);
            return obsoleteCount;
        }

        public static int DeleteReplaysThatDoNotHaveTheActualNamesInIt(this List<Replay> replays, IEnumerable<string> names)
        {
            var newReplayList = new List<Replay>();
            foreach(var name in names)
            {
                foreach(var replay in replays)
                {
                    if(replay.HasNameInIt(name))
                        newReplayList.Add(replay);
                }
            }
            var obsoleteCount = replays.Count - newReplayList.Count;
            replays.Clear();
            replays.AddRange(newReplayList);
            return obsoleteCount;
        }

        public static void DeleteReplaysThatAreNotInTimeRange(this List<Replay> replays, DateTime start, DateTime end)
        {
            var replaysInTimeZone = replays.GetReplaysInTimeZone(start, end);
            replays.Clear();
            replays.AddRange(replaysInTimeZone);
        }

        public static void TrimReplaysToCap(this List<Replay> replays, int filterReplayCap)
        {
            if (replays.Count <= filterReplayCap)
                return;
            var replayPile = new List<Replay>(replays);
            replays.Clear();
            for (var i = 0; i < filterReplayCap; i++)
                replays.Add(replayPile[i]);
        }
    }
}
