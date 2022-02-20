using MongoDB.Driver;

using RLStats.MongoDBSupport.Models;

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
        private readonly MongoClient client;
        private readonly IMongoDatabase db;

        public int CacheSize { get; set; } = 0;
        public int CacheHits { get; set; } = 0;
        public int CacheMisses { get; set; } = 0;

        public RLStatsMongoDatabase(DatabaseSettings settings)
        {
            client = new MongoClient(settings.ConnectionString);
            db = client.GetDatabase(settings.DatabaseName);
        }

        public string GetBallchasingToken()
        {
            var tokenCollection = db.GetCollection<TokenWrapper>(BallchasingTokenCollectionName);
            if (tokenCollection.CountDocuments(_ => true).Equals(0))
                return string.Empty;
            var tokenInfo = tokenCollection.Find(_ => true).First();
            if (tokenInfo is null)
                return string.Empty;
            return tokenInfo.Token;
        }

        public void SetBallchasingToken(string ballchasingToken)
        {
            db.DropCollection(BallchasingTokenCollectionName);
            var tokenCollection = db.GetCollection<TokenWrapper>(BallchasingTokenCollectionName);
            tokenCollection.InsertOne(new TokenWrapper { Token = ballchasingToken });
        }

        public void ClearCache() { }

        public bool IsReplayInDatabase(Replay replay)
        {
            throw new NotImplementedException();
        }

        public Task<AdvancedReplay> LoadReplayAsync(string id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void SaveReplayAsync(AdvancedReplay replay)
        {
            throw new NotImplementedException();
        }

        public void AddTheOtherReplaysToTheDataPack(HashSet<Replay> hashSet, APIRequestFilter filter)
        {
            throw new NotImplementedException();
        }

        public bool HasCacheFile(APIRequestFilter filter)
        {
            throw new NotImplementedException();
        }

        public bool HasOneReplayInFile(IEnumerable<Replay> replays, APIRequestFilter filter)
        {
            throw new NotImplementedException();
        }

        public void StoreReplaysInCache(IEnumerable<Replay> replays, APIRequestFilter filter)
        {
            throw new NotImplementedException();
        }

        public ServiceInfo GetServiceInfo()
        {
            throw new NotImplementedException();
        }

        public void SaveServiceInfo(ServiceInfo info)
        {
            throw new NotImplementedException();
        }
    }
}