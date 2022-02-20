using MongoDB.Driver;

using RLStatsClasses;
using RLStatsClasses.Interfaces;
using RLStatsClasses.Models;
using RLStatsClasses.Models.ReplayModels;
using RLStatsClasses.Models.ReplayModels.Advanced;

namespace RLStats.MongoDBSupport
{
    public class RLStatsMongoDatabase : ISaveBallchasingToken, IDatabase, IReplayCache, IServiceInfoIO
    {
        private const string BallchasingTokenCollectionName = "BallchasingTokens";
        private const string AdvancedReplayCollectionName = "AdvancedReplays";
        private const string ServiceInfoCollectionName = "ServiceInfo";
        private const string ReplayCacheCollectionName = "ReplayCache";
        private readonly MongoClient client;
        private readonly IMongoDatabase db;

        public RLStatsMongoDatabase(DatabaseSettings settings)
        {
            client = new MongoClient(settings.ConnectionString);
            db = client.GetDatabase(settings.DatabaseName);
        }
        #region ISaveBallchasingToken
        public string GetBallchasingToken()
        {
            var tokenCollection = db.GetCollection<Wrapper<string>>(BallchasingTokenCollectionName);
            if (IsEmpty(tokenCollection))
                return string.Empty;
            var tokenInfo = tokenCollection.Find(_ => true).FirstOrDefault();
            if (tokenInfo is null)
                return string.Empty;
            return tokenInfo.Value ?? string.Empty;
        }

        public void SetBallchasingToken(string ballchasingToken)
        {
            db.DropCollection(BallchasingTokenCollectionName);
            var tokenCollection = db.GetCollection<Wrapper<string>>(BallchasingTokenCollectionName);
            tokenCollection.InsertOne(new Wrapper<string> { Value = ballchasingToken });
        }
        #endregion
        #region IDatabase
        public int CacheSize { get; set; } = 0;
        public int CacheHits { get; set; } = 0;
        public int CacheMisses { get; set; } = 0;
        public void ClearCache() { }

        public bool IsReplayInDatabase(Replay replay)
        {
            var coll = db.GetCollection<Wrapper<AdvancedReplay>>(AdvancedReplayCollectionName);
            if (IsEmpty(coll))
                return false;
            var wrapper = coll.Find(aReplay => aReplay.Value.Id.Equals(replay.Id)).FirstOrDefault();
            if (wrapper is null)
                return false;
            return true;
        }

        public async Task<AdvancedReplay> LoadReplayAsync(string id, CancellationToken cancellationToken)
        {
            var coll = db.GetCollection<Wrapper<AdvancedReplay>>(AdvancedReplayCollectionName);
            if (IsEmpty(coll))
                return null;
            var wrapper = (await coll.FindAsync(aReplay => aReplay.Value.Id.Equals(id), cancellationToken: cancellationToken)).FirstOrDefault();
            if (wrapper is null)
                return null;
            return wrapper.Value;
        }

        public async void SaveReplayAsync(AdvancedReplay replay)
        {
            var coll = db.GetCollection<Wrapper<AdvancedReplay>>(AdvancedReplayCollectionName);
            await coll.InsertOneAsync(new Wrapper<AdvancedReplay> { Value = replay });
        }
        #endregion
        #region IReplayCache
        public void AddTheOtherReplaysToTheDataPack(HashSet<Replay> hashSet, APIRequestFilter filter)
        {
            var coll = db.GetCollection<Wrapper<ReplayCacheDocument>>(ReplayCacheCollectionName);
            var wrapper = coll.Find(doc => doc.Value.Url.Equals(filter.GetApiUrl())).FirstOrDefault();
            if (wrapper is null || wrapper.Value is null)
                return;
            foreach(var replay in wrapper.Value.Replays)
                hashSet.Add(replay);
        }

        public bool HasCacheFile(APIRequestFilter filter)
        {
            var coll = db.GetCollection<Wrapper<ReplayCacheDocument>>(ReplayCacheCollectionName);
            var wrapper = coll.Find(doc => doc.Value.Url.Equals(filter.GetApiUrl())).FirstOrDefault();
            return wrapper is not null;
        }

        public bool HasOneReplayInFile(IEnumerable<Replay> replays, APIRequestFilter filter)
        {
            var coll = db.GetCollection<Wrapper<ReplayCacheDocument>>(ReplayCacheCollectionName);
            var wrapper = coll.Find(doc => doc.Value.Url.Equals(filter.GetApiUrl())).FirstOrDefault();
            if (wrapper is null || wrapper.Value is null)
                return false;
            foreach(var replay in replays)
            {
                if (wrapper.Value.Replays.Contains(replay))
                    return true;
            }
            return false;
        }

        public void StoreReplaysInCache(IEnumerable<Replay> replays, APIRequestFilter filter)
        {
            var coll = db.GetCollection<Wrapper<ReplayCacheDocument>>(ReplayCacheCollectionName);
            var document = new ReplayCacheDocument
            {
                Replays = replays,
                Url = filter.GetApiUrl()
            };
            
            coll.FindOneAndDelete(doc => doc.Value.Url.Equals(filter.GetApiUrl()));
            coll.InsertOne(new Wrapper<ReplayCacheDocument> { Value = document });
        }
        #endregion
        #region IServiceInfoIO
        public ServiceInfo GetServiceInfo()
        {
            var coll = db.GetCollection<Wrapper<ServiceInfo>>(ServiceInfoCollectionName);
            if (IsEmpty(coll))
                return new ServiceInfo() { Available = false };
            var wrapper = coll.Find(_ => true).FirstOrDefault();
            if (wrapper is null || wrapper.Value is null)
                return new ServiceInfo() { Available = false };
            return wrapper.Value;
        }

        public void SaveServiceInfo(ServiceInfo info)
        {
            db.DropCollection(ServiceInfoCollectionName);
            var coll = db.GetCollection<Wrapper<ServiceInfo>>(ServiceInfoCollectionName);
            coll.InsertOne(new Wrapper<ServiceInfo> { Value = info });
        }
        #endregion
        private static bool IsEmpty<T>(IMongoCollection<T> collection)
        {
            return collection.CountDocuments(_ => true).Equals(0);
        }
    }
}