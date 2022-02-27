using MongoDB.Driver;

namespace RLStats.MongoDBSupport
{
    public class DatabaseSettings
    {
        public MongoClientSettings MongoSettings { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
    }
}