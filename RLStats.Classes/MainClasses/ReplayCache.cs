using Newtonsoft.Json;

using RLStats_Classes.Models;

using System;
using System.Collections.Generic;
using System.IO;

namespace RLStats_Classes.MainClasses
{
    public class ReplayCache
    {
        class CacheObject
        {
            public string Url { get; set; }
            public CollectReplaysResponse Response { get; set; } = new CollectReplaysResponse();
        }

        public void StoreReplaysInCache(CollectReplaysResponse response, APIRequestFilter filter)
        {
            var guid = Guid.NewGuid().ToString();
            if (HasCacheFile(filter))
                guid = GetFileName(filter);
            var fileName = Path.Combine(RLConstants.ReplayCacheFolder, guid);
            var cacheObject = new CacheObject
            {
                Url = filter.GetApiUrl(),
                Response = response
            };
            var compressedBytes = Compressor.CompressString(JsonConvert.SerializeObject(cacheObject));
            File.WriteAllBytes(fileName, compressedBytes);
        }

        private string GetFileName(APIRequestFilter filter)
        {
            var cacheFiles = Directory.GetFiles(RLConstants.ReplayCacheFolder);
            foreach (var cacheFile in cacheFiles)
            {
                var compressedBytes = File.ReadAllBytes(cacheFile);
                var jsonString = Compressor.DecompressBytesToString(compressedBytes);
                var cacheObject = JsonConvert.DeserializeObject<CacheObject>(jsonString);
                if (filter.GetApiUrl().Equals(cacheObject.Url))
                    return Path.GetFileName(cacheFile);
            }
            return Guid.NewGuid().ToString();
        }

        public bool HasCacheFile(APIRequestFilter filter)
        {
            var cacheFiles = Directory.GetFiles(RLConstants.ReplayCacheFolder);
            foreach (var cacheFile in cacheFiles)
            {
                var url = GetCachObjectOutOfCacheFile(filter).Url;
                if (filter.GetApiUrl().Equals(url))
                    return true;
            }
            return false;
        }

        public bool HasOneReplayInFile(IEnumerable<Replay> replays, APIRequestFilter filter)
        {
            var cacheObject = GetCachObjectOutOfCacheFile(filter);
            var cachedReplays = new List<Replay>(cacheObject.Response.Replays);
            foreach (var replay in replays)
            {
                if (cachedReplays.DoesListContainReplay(replay))
                {
                    return true;
                }
            }
            return false;
        }

        private CacheObject GetCachObjectOutOfCacheFile(APIRequestFilter filter)
        {
            var cacheFiles = Directory.GetFiles(RLConstants.ReplayCacheFolder);
            foreach (var cacheFile in cacheFiles)
            {
                var compressedBytes = File.ReadAllBytes(cacheFile);
                var jsonString = Compressor.DecompressBytesToString(compressedBytes);
                var cacheObject = JsonConvert.DeserializeObject<CacheObject>(jsonString);
                if (filter.GetApiUrl().Equals(cacheObject.Url))
                    return cacheObject;
            }
            return new CacheObject();
        }

        public void AddTheOtherReplaysToTheDataPack(ApiDataPack dataPack, APIRequestFilter filter)
        {
            var cacheObject = GetCachObjectOutOfCacheFile(filter);
            dataPack.Replays.AddRange(cacheObject.Response.Replays);
            _ = dataPack.Replays.DeleteObsoleteReplays();
        }
    }
}
