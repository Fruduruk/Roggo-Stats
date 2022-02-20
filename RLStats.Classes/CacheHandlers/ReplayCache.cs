using RLStats_Classes.Models;

using System;
using System.Collections.Generic;
using System.IO;

namespace RLStats_Classes.MainClasses.CacheHandlers
{
    public class ReplayCache : WithIndexFile<CacheEntry>
    {
        public ReplayCache() : base(compressed: false, Path.Combine(RLConstants.ReplayCacheFolder, "ContentRegistry.txt"))
        {

        }

        public void StoreReplaysInCache(CollectReplaysResponse response, APIRequestFilter filter)
        {
            var fileName = $"{Guid.NewGuid()}.7z";
            var hasCacheFile = HasCacheFile(filter);
            if (hasCacheFile)
                fileName = GetFileName(filter);
            var filePath = Path.Combine(RLConstants.ReplayCacheFolder, fileName);

            File.WriteAllBytes(filePath, Compressor.ConvertObject(new List<Replay>(response.Replays), false));
            if (!hasCacheFile)
                IndexCollection.Add(new CacheEntry { FileName = fileName, URL = filter.GetApiUrl() });
        }

        private string GetFileName(APIRequestFilter filter)
        {
            lock (IndexCollection)
            {
                foreach (var entry in IndexCollection)
                {
                    if (filter.GetApiUrl().Equals(entry.URL))
                        return Path.Combine(RLConstants.ReplayCacheFolder, entry.FileName);
                }
            }
            return string.Empty;
        }

        public bool HasCacheFile(APIRequestFilter filter)
        {
            lock (IndexCollection)
            {
                foreach (var entry in IndexCollection)
                {
                    if (filter.GetApiUrl().Equals(entry.URL))
                        return true;
                }
            }
            return false;
        }

        public bool HasOneReplayInFile(IEnumerable<Replay> replays, APIRequestFilter filter)
        {
            try
            {
                var cacheReplays = new HashSet<Replay>(GetReplaysOutOfCacheFile(filter));
                foreach (var replay in replays)
                {
                    if (cacheReplays.Contains(replay))
                    {
                        return true;
                    }
                }
            }
            catch (FileNotFoundException)
            {
                RemoveFalseEntryFromIndexCollection(filter);
                return false;
            }
            return false;
        }

        private void RemoveFalseEntryFromIndexCollection(APIRequestFilter filter)
        {
            CacheEntry falseEntry = null;
            lock (IndexCollection)
            {
                foreach (var entry in IndexCollection)
                {
                    if (filter.GetApiUrl().Equals(entry.URL))
                    {
                        falseEntry = entry;
                    }
                }
                if (falseEntry != null)
                    IndexCollection.Remove(falseEntry);
            }
        }

        private List<Replay> GetReplaysOutOfCacheFile(APIRequestFilter filter)
        {
            var fileName = GetFileName(filter);
            return Compressor.ConvertObject<List<Replay>>(File.ReadAllBytes(fileName), false);
        }

        public void AddTheOtherReplaysToTheDataPack(HashSet<Replay> hashSet, APIRequestFilter filter)
        {
            hashSet.UnionWith(GetReplaysOutOfCacheFile(filter));
        }
    }
}
