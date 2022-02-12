using Newtonsoft.Json;

using RLStats_Classes.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        public static int DeleteReplaysThatDoNotHaveTheActualNamesInIt(this HashSet<Replay> replays, IEnumerable<string> names)
        {
            if (!names.Any())
                return 0;
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
            replays.UnionWith(newReplayList);
            return obsoleteCount;
        }

        public static void DeleteReplaysThatAreNotInTimeRange(this HashSet<Replay> replays, DateTime start, DateTime end)
        {
            var replaysInTimeZone = replays.GetReplaysInTimeZone(start, end);
            replays.Clear();
            replays.UnionWith(replaysInTimeZone);
        }

        public static void TrimReplaysToCap(this HashSet<Replay> replays, int filterReplayCap)
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
