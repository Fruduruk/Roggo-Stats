﻿using RLStats_Classes.MainClasses;
using System;
using System.Collections.Generic;

namespace RLStats_Classes.Models
{
    public class ApiDataPack
    {
        public List<Replay> Replays { get; set; }
        public int ReplayCount { get; set; }
        public dynamic Next { get; set; }
        public bool Success { get; set; }
        public Exception Ex { get; set; }
        public string ReceivedString { get; set; } = string.Empty;

        public int DeleteObsoleteReplays()
        {
            var newReplayList = new List<Replay>();
            foreach (var replay in Replays)
                if (!DataPackMerger.DoesListContainReplay(newReplayList, replay))
                    newReplayList.Add(replay);
            var obsoleteCount = Replays.Count-newReplayList.Count;
            Replays = newReplayList;
            return obsoleteCount;
        }

        public void DeleteReplaysThatAreNotInTimeRange(DateTime start, DateTime end)
        {
            Replays = DataPackMerger.GetReplaysForTimeZone(Replays, start, end);
        }

        public void TrimReplaysToCap(int filterReplayCap)
        {
            if(Replays.Count <= filterReplayCap)
                return;
            var replays = new List<Replay>();
            for (var i = 0; i < filterReplayCap; i++)
            {
                var replay = Replays[i];
                replays.Add(replay);
            }

            Replays = replays;
        }
    }
}