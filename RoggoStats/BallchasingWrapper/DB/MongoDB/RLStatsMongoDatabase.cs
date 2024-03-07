using BallchasingWrapper.Interfaces;
using BallchasingWrapper.Models;
using BallchasingWrapper.Models.ReplayModels;
using BallchasingWrapper.Models.ReplayModels.Advanced;
using MongoDB.Driver;

namespace BallchasingWrapper.DB.MongoDB
{
    public class RlStatsMongoDatabase : IDatabase, IReplayCache, IServiceInfoIO
    {
        private const string AdvancedReplayCollectionName = "AdvancedReplays";
        private const string ServiceInfoCollectionName = "ServiceInfo";
        private const string ReplayCacheCollectionName = "ReplayCache";
        private MongoClient Client { get; }
        private IMongoDatabase Database { get; }

        public RlStatsMongoDatabase(DatabaseSettings settings)
        {
            Client = new MongoClient(settings.MongoSettings);
            Database = Client.GetDatabase(settings.DatabaseName);
        }

        #region IDatabase
        public async Task<IEnumerable<AdvancedReplay>> LoadReplaysAsync(IEnumerable<string> ids, CancellationToken cancellationToken, ProgressState progressState)
        {
            var coll = Database.GetCollection<Wrapper<AdvancedReplay>>(AdvancedReplayCollectionName);
            progressState.CurrentMessage = "Waiting for MongoDB response...";
            var wrappers = (await coll.FindAsync(wrapper => ids.Contains(wrapper.Value.Id), cancellationToken: cancellationToken)).ToList(cancellationToken: cancellationToken);
            if (wrappers is null)
                return Enumerable.Empty<AdvancedReplay>();
            return wrappers.Select(w => w.Value);
        }

        public async void SaveReplayAsync(AdvancedReplay replay)
        {
            var coll = Database.GetCollection<Wrapper<AdvancedReplay>>(AdvancedReplayCollectionName);
            await coll.InsertOneAsync(new Wrapper<AdvancedReplay> { Value = replay });
        }
        #endregion
        #region IReplayCache
        public void AddTheOtherReplaysToTheDataPack(HashSet<Replay> hashSet, ApiUrlCreator filter)
        {
            var coll = Database.GetCollection<Wrapper<ReplayCacheDocument>>(ReplayCacheCollectionName);
            var wrapper = coll.Find(doc => doc.Value.Url.Equals(filter.Urls.First())).FirstOrDefault();
            if (wrapper is null || wrapper.Value is null)
                return;
            foreach(var replay in wrapper.Value.Replays)
                hashSet.Add(replay);
        }

        public bool HasCacheFile(ApiUrlCreator filter)
        {
            var coll = Database.GetCollection<Wrapper<ReplayCacheDocument>>(ReplayCacheCollectionName);
            var wrapper = coll.Find(doc => doc.Value.Url.Equals(filter.Urls.First())).FirstOrDefault();
            return wrapper is not null;
        }

        public bool HasOneReplayInFile(IEnumerable<Replay> replays, ApiUrlCreator filter)
        {
            var coll = Database.GetCollection<Wrapper<ReplayCacheDocument>>(ReplayCacheCollectionName);
            var wrapper = coll.Find(doc => doc.Value.Url.Equals(filter.Urls.First())).FirstOrDefault();
            if (wrapper is null || wrapper.Value is null)
                return false;
            foreach(var replay in replays)
            {
                if (wrapper.Value.Replays.Contains(replay))
                    return true;
            }
            return false;
        }

        public void StoreReplaysInCache(IEnumerable<Replay> replays, ApiUrlCreator filter)
        {
            var coll = Database.GetCollection<Wrapper<ReplayCacheDocument>>(ReplayCacheCollectionName);
            var document = new ReplayCacheDocument
            {
                Replays = replays,
                Url = filter.Urls.First()
            };
            
            coll.FindOneAndDelete(doc => doc.Value.Url.Equals(filter.Urls.First()));
            coll.InsertOne(new Wrapper<ReplayCacheDocument> { Value = document });
        }
        #endregion
        #region IServiceInfoIO
        public ServiceInfo GetServiceInfo()
        {
            var coll = Database.GetCollection<Wrapper<ServiceInfo>>(ServiceInfoCollectionName);
            if (IsEmpty(coll))
                return new ServiceInfo() { Available = false };
            var wrapper = coll.Find(_ => true).FirstOrDefault();
            if (wrapper is null || wrapper.Value is null)
                return new ServiceInfo() { Available = false };
            return wrapper.Value;
        }

        public void SaveServiceInfo(ServiceInfo info)
        {
            Database.DropCollection(ServiceInfoCollectionName);
            var coll = Database.GetCollection<Wrapper<ServiceInfo>>(ServiceInfoCollectionName);
            coll.InsertOne(new Wrapper<ServiceInfo> { Value = info });
        }
        #endregion
        private static bool IsEmpty<T>(IMongoCollection<T> collection)
        {
            return collection.CountDocuments(_ => true).Equals(0);
        }
    }
}