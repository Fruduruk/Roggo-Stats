using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BallchasingWrapper.DB.MongoDB
{
    public class Wrapper<T> where T : class
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public T Value { get; set; } = null!;

        public static IEnumerable<Wrapper<T>> WrapList(IEnumerable<T> listToWrap)
        {
            var wrappedList = new List<Wrapper<T>>();
            foreach (var item in listToWrap)
            {
                wrappedList.Add(new Wrapper<T>
                {
                    Value = item
                });
            }
            return wrappedList;
        }
    }
}
