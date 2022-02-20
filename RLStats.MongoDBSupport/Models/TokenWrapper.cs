using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RLStats.MongoDBSupport.Models
{
    public class TokenWrapper
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string Token { get; set; } = null!;
    }
}
