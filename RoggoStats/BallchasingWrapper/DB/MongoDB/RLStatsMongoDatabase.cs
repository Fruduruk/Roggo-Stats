using BallchasingWrapper.Interfaces;
using BallchasingWrapper.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using AdvancedReplay = BallchasingWrapper.Models.ReplayModels.Advanced.AdvancedReplay;
using Replay = BallchasingWrapper.Models.ReplayModels.Replay;

namespace BallchasingWrapper.DB.MongoDB
{
    public class RlStatsMongoDatabase : IReplayCache, IDatabase, IBackgroundDownloaderConfig, ILogger
    {
        private const string AdvancedReplayCollectionName = "AdvancedReplays";
        private const string BackgroundOperationsCollectionName = "BackgroundOperations";
        private const string SimpleReplaysCollectionName = "SimpleReplays";
        private const string LogCollectionName = "Logs";

        private readonly IMongoCollection<BsonDocument> _logCollection;
        private readonly MongoClient _client;
        private readonly IMongoDatabase _database;

        public RlStatsMongoDatabase(DatabaseSettings settings)
        {
            _client = new MongoClient(settings.MongoSettings);
            _database = _client.GetDatabase(settings.DatabaseName);
            if (!IsMongoDbAvailable().Result)
            {
                throw new Exception("No MongoDb available.");
            }

            _logCollection = _database.GetCollection<BsonDocument>(LogCollectionName);
        }

        private async Task<bool> IsMongoDbAvailable(CancellationToken cancellationToken = default)
        {
            try
            {
                var pingCommand = new BsonDocument("ping", 1);
                await _database.RunCommandAsync<BsonDocument>(pingCommand, cancellationToken: cancellationToken);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #region IDatabase

        public async Task<IEnumerable<AdvancedReplay>> LoadReplaysByIdsAsync(IEnumerable<string> ids,
            CancellationToken cancellationToken = default)
        {
            var coll = _database.GetCollection<Wrapper<AdvancedReplay>>(AdvancedReplayCollectionName);
            var wrappers =
                (await coll.FindAsync(wrapper => ids.Contains(wrapper.Value.Id), cancellationToken: cancellationToken))
                .ToList(cancellationToken: cancellationToken);
            if (wrappers is null)
                return Enumerable.Empty<AdvancedReplay>();
            return wrappers.Select(w => w.Value);
        }

        public async Task SaveReplayAsync(AdvancedReplay replay)
        {
            await Task.Run(() =>
            {
                var coll = _database.GetCollection<Wrapper<AdvancedReplay>>(AdvancedReplayCollectionName);
                lock (_client)
                {
                    if (coll.Find(document => document.Value.Id == replay.Id).FirstOrDefault() is null)
                        coll.InsertOne(new Wrapper<AdvancedReplay> { Value = replay });
                }
            });
        }

        #endregion

        #region IReplayCache

        public async Task<HashSet<Replay>?> LoadCachedReplays(ApiUrlCreator filter)
        {
            var coll = _database.GetCollection<Wrapper<ReplayCacheDocument>>(SimpleReplaysCollectionName);
            var filterString = filter.ToString();
            var wrapper = await (await coll.FindAsync(doc => doc.Value.FilterString == filterString))
                .FirstOrDefaultAsync();
            return wrapper?.Value.Replays.ToHashSet();
        }

        public async Task WriteReplayCache(ApiUrlCreator filter, IEnumerable<Replay> replays)
        {
            var coll = _database.GetCollection<Wrapper<ReplayCacheDocument>>(SimpleReplaysCollectionName);
            var filterString = filter.ToString();
            var document = new ReplayCacheDocument
            {
                Replays = replays,
                FilterString = filterString,
            };
            await coll.FindOneAndDeleteAsync(doc => doc.Value.FilterString == filterString);
            await coll.InsertOneAsync(new Wrapper<ReplayCacheDocument> { Value = document });
        }

        #endregion

        #region IBackgroundDownloaderConfig

        public async Task<IEnumerable<Grpc.BackgroundDownloadOperation>> LoadOperationsAsync()
        {
            var coll =
                _database.GetCollection<Wrapper<Grpc.BackgroundDownloadOperation>>(BackgroundOperationsCollectionName);
            var operations = (await coll.FindAsync(_ => true)).ToList().Select(wrapper => wrapper.Value);
            return operations;
        }

        public async Task<bool> SaveOperationAsync(Grpc.BackgroundDownloadOperation operation)
        {
            return await Task.Run(() =>
            {
                var coll =
                    _database.GetCollection<Wrapper<Grpc.BackgroundDownloadOperation>>(
                        BackgroundOperationsCollectionName);
                lock (_client)
                {
                    if (coll.Find(wrapper =>
                            wrapper.Value.Identity.IdentityType == operation.Identity.IdentityType &&
                            wrapper.Value.Identity.NameOrId == operation.Identity.NameOrId).Any())
                    {
                        return false;
                    }

                    coll.InsertOne(new Wrapper<Grpc.BackgroundDownloadOperation>
                    {
                        Value = operation
                    });
                    return true;
                }
            });
        }

        public async Task<bool> DeleteOperationAsync(Grpc.Identity identity)
        {
            return await Task.Run(() =>
            {
                var coll =
                    _database.GetCollection<Wrapper<Grpc.BackgroundDownloadOperation>>(
                        BackgroundOperationsCollectionName);
                lock (_client)
                {
                    if (coll.Find(wrapper =>
                            wrapper.Value.Identity.IdentityType == identity.IdentityType &&
                            wrapper.Value.Identity.NameOrId == identity.NameOrId).Any())
                    {
                        coll.DeleteOne(wrapper =>
                            wrapper.Value.Identity.IdentityType == identity.IdentityType &&
                            wrapper.Value.Identity.NameOrId == identity.NameOrId);
                        return true;
                    }

                    return false;
                }
            });
        }

        #endregion

        #region ILogger

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var document = new BsonDocument
            {
                { "Level", logLevel.ToString() },
                { "Event", $"{eventId.Id.ToString()}-{eventId.Name ?? "None"}" },
                { "Message", formatter(state, exception) },
                { "Exception", exception?.ToString() ?? string.Empty },
                { "TimeStamp", DateTime.UtcNow }
            };

            _logCollection.InsertOneAsync(document);
        }

        class RlStatsMongoDbLoggerProvider : ILoggerProvider
        {
            private readonly RlStatsMongoDatabase _db;

            public RlStatsMongoDbLoggerProvider(RlStatsMongoDatabase db)
            {
                _db = db;
            }

            public void Dispose()
            {
            }

            public ILogger CreateLogger(string categoryName)
            {
                return _db;
            }
        }

        public ILoggerProvider ToLoggerProvider()
        {
            return new RlStatsMongoDbLoggerProvider(this);
        }

        #endregion
    }
}