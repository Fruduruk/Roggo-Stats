using BallchasingWrapper.BusinessLogic;
using BallchasingWrapper.Interfaces;
using BallchasingWrapper.Models;
using BallchasingWrapper.Models.ReplayModels;

namespace BallchasingWrapper.DB.LegacyFileDB
{
    public class ReplayCache : WithIndexFile<CacheEntry>, IReplayCache
    {
        public ReplayCache() : base(compressed: false, Path.Combine(RLConstants.ReplayCacheFolder, "ContentRegistry.txt"))
        {

        }

        public void StoreReplaysInCache(IEnumerable<Replay> replays, ApiUrlCreator filter)
        {
            var fileName = $"{Guid.NewGuid()}.7z";
            var hasCacheFile = HasCacheFile(filter);
            if (hasCacheFile)
                fileName = GetFileName(filter);
            var filePath = Path.Combine(RLConstants.ReplayCacheFolder, fileName);

            File.WriteAllBytes(filePath, Compressor.ConvertObject(new List<Replay>(replays), false));
            if (!hasCacheFile)
                IndexCollection.Add(new CacheEntry { FileName = fileName, URL = filter.Urls.First() });
        }

        private string GetFileName(ApiUrlCreator filter)
        {
            lock (IndexCollection)
            {
                foreach (var entry in IndexCollection)
                {
                    if (filter.Urls.First().Equals(entry.URL))
                        return Path.Combine(RLConstants.ReplayCacheFolder, entry.FileName);
                }
            }
            return string.Empty;
        }

        public bool HasCacheFile(ApiUrlCreator filter)
        {
            lock (IndexCollection)
            {
                foreach (var entry in IndexCollection)
                {
                    if (filter.Urls.First().Equals(entry.URL))
                        return true;
                }
            }
            return false;
        }

        public bool HasOneReplayInFile(IEnumerable<Replay> replays, ApiUrlCreator filter)
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

        private void RemoveFalseEntryFromIndexCollection(ApiUrlCreator filter)
        {
            CacheEntry falseEntry = null;
            lock (IndexCollection)
            {
                foreach (var entry in IndexCollection)
                {
                    if (filter.Urls.First().Equals(entry.URL))
                    {
                        falseEntry = entry;
                    }
                }
                if (falseEntry != null)
                    IndexCollection.Remove(falseEntry);
            }
        }

        private List<Replay> GetReplaysOutOfCacheFile(ApiUrlCreator filter)
        {
            var fileName = GetFileName(filter);
            return Compressor.ConvertObject<List<Replay>>(File.ReadAllBytes(fileName), false);
        }

        public void AddTheOtherReplaysToTheDataPack(HashSet<Replay> hashSet, ApiUrlCreator filter)
        {
            hashSet.UnionWith(GetReplaysOutOfCacheFile(filter));
        }
    }
}
