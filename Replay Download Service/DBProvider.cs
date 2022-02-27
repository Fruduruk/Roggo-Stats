using RLStats.MongoDBSupport;

using RLStatsClasses;
using RLStatsClasses.CacheHandlers;
using RLStatsClasses.Interfaces;

namespace ReplayDownloadService
{
    public class DBProvider
    {
        public static DBProvider Instance { get; private set; }
        public DBType DatabaseType { get; }

        public static void CreateInstance(DBType dbType, DatabaseSettings settings = null)
        {
            if (Instance is null)
                Instance = new DBProvider(dbType, settings);
        }

        private RLStatsMongoDatabase MongoDB { get; set; }
        public DBProvider(DBType dbType, DatabaseSettings settings = null)
        {
            DatabaseType = dbType;
            if (dbType.Equals(DBType.MongoDB))
                MongoDB = new RLStatsMongoDatabase(settings);
        }

        public IServiceInfoIO GetServiceInfoDB()
        {
            return DatabaseType switch
            {
                DBType.Legacy => new ServiceInfoIO(),
                DBType.MongoDB => MongoDB,
                _ => null
            };
        }

        public IReplayCache GetReplayCacheDB()
        {
            return DatabaseType switch
            {
                DBType.Legacy => new ReplayCache(),
                DBType.MongoDB => MongoDB,
                _ => null
            };
        }

        public ISaveBallchasingToken GetBallchasingTokenDB()
        {
            return DatabaseType switch
            {
                DBType.Legacy => new RLConstants(),
                DBType.MongoDB => MongoDB,
                _ => null
            };
        }

        public IDatabase GetAdvancedReplayDB()
        {
            return DatabaseType switch
            {
                DBType.Legacy => new Database(),
                DBType.MongoDB => MongoDB,
                _ => null
            };
        }
    }
}
