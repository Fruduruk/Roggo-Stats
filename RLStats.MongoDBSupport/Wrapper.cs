using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RLStats.MongoDBSupport
{
    public class Wrapper<T> where T : class
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public T Value { get; set; } = null!;
    }
}
