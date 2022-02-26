using RLStats.MongoDBSupport;

using RLStatsClasses;
using RLStatsClasses.CacheHandlers;
using RLStatsClasses.Interfaces;

namespace RocketLeagueStats
{
    public class DBProvider
    {
        public static DBProvider Instance { get; private set; }
        public DBType DatabaseType { get; }

        public static void CreateInstance(DatabaseSettings settings)
        {
            if (Instance is null)
                Instance = new DBProvider(DBType.MongoDB, settings);
        }

        public static void CreateInstance()
        {
            if (Instance is null)
                Instance = new DBProvider(DBType.Legacy);
        }

        private RLStatsMongoDatabase MongoDB { get; set; }

        private DBProvider(DBType dbType, DatabaseSettings settings = null)
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
