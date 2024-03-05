using MongoDB.Driver;

namespace BallchasingWrapper.DB.MongoDB
{
    public class DatabaseSettings
    {
        public MongoClientSettings MongoSettings { get; init; } = null!;
        public string DatabaseName { get; init; } = null!;
    }
}