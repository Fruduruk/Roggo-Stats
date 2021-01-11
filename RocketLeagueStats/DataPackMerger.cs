using System;
using System.Collections.Generic;

namespace RocketLeagueStats
{
    public class DataPackMerger
    {
        private const string NoDataPackToMerge = "no datapack to merge";
        public List<ApiDataPack> DataPacks = new List<ApiDataPack>();
        private List<Replay> commonReplays;

        public ApiDataPack GetDataMerge()
        {
            if (DataPacks.Count == 1)
                return DataPacks[0];
            if (DataPacks.Count == 0)
                return new ApiDataPack()
                {
                    Success = false,
                    ReplayCount = 0,
                    Replays = new List<Replay>(),
                    Ex = new Exception(NoDataPackToMerge),
                    ReceivedString = NoDataPackToMerge
                };
            foreach (ApiDataPack dataPack in DataPacks)
                if (!dataPack.Success)
                    return dataPack;

            commonReplays = new List<Replay>();
            for (int j = 0; j < DataPacks[0].Replays.Count; j++)
            {
                Replay r = DataPacks[0].Replays[j];
                if (IfEverybodyHas(r))
                    commonReplays.Add(r);
            }
            return new ApiDataPack()
            {
                Success = true,
                Replays = commonReplays
            };

        }

        private bool IfEverybodyHas(Replay r)
        {
            for (int i = 1; i < DataPacks.Count; i++)
                if (!DoesListContainReplay(DataPacks[i].Replays, r))
                {
                    return false;
                }

            return true;
        }

        public static List<Replay> GetReplaysForTimeZone(List<Replay> replays, DateTime start, DateTime end)
        {
            List<Replay> replaysInTimeZone = new List<Replay>();
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
                if (r.ID.Equals(replay.ID))
                    return true;
            return false;
        }
    }
}
