using MongoDB.Driver;

namespace BallchasingWrapper.DB.MongoDB
{
    public class DatabaseSettings
    {
        public MongoClientSettings MongoSettings { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
    }
}