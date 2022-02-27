using MongoDB.Driver;

using RLStats.MongoDBSupport;

using System.Collections.Generic;
using System.Linq;

namespace Discord_Bot.Configuration
{
    public class RLStatsDiscordBotMongoDatabase : RLStatsMongoDatabase, IConfigHandler<Subscription>, IConfigHandler<UserFavorite>
    {
        private const string UserFavoriteCollectionName = "DiscordBotUserFavorites";
        private const string SubscriptionCollectionName = "DiscordBotSubscriptions";

        private IMongoCollection<Wrapper<UserFavorite>> UserFavorites { get; set; }
        private IMongoCollection<Wrapper<Subscription>> Subscriptions { get; set; }

        public RLStatsDiscordBotMongoDatabase(DatabaseSettings settings) : base(settings)
        {
            UserFavorites = Database.GetCollection<Wrapper<UserFavorite>>(UserFavoriteCollectionName);
            Subscriptions = Database.GetCollection<Wrapper<Subscription>>(SubscriptionCollectionName);
        }
        
        #region UserFavorite
        public void AddConfigEntry(UserFavorite entry) => UserFavorites.InsertOne(new Wrapper<UserFavorite> { Value = entry });
        public bool HasConfigEntryInIt(UserFavorite entry) => UserFavorites.Find(w => w.Value.Equals(entry)).FirstOrDefault() is not null;
        public void RemoveConfigEntry(UserFavorite entry) => UserFavorites.FindOneAndDelete(w => w.Value.Equals(entry));
        public void SetConfig(List<UserFavorite> value)
        {
            UserFavorites.DeleteMany(_ => true);
            UserFavorites.InsertMany(Wrapper<UserFavorite>.WrapList(value));
        }
        List<UserFavorite> IConfigHandler<UserFavorite>.GetConfig()
        {
            var list = UserFavorites.Find(_ => true).ToList();
            if(list is not null)
                return list.Select(w => w.Value).ToList();
            return new List<UserFavorite>();
        }
        #endregion

        #region Subscription
        public void AddConfigEntry(Subscription entry) => Subscriptions.InsertOne(new Wrapper<Subscription> { Value = entry });
        public bool HasConfigEntryInIt(Subscription entry) => Subscriptions.Find(w => w.Value.Equals(entry)).FirstOrDefault() is not null;
        public void RemoveConfigEntry(Subscription entry) => Subscriptions.FindOneAndDelete(w => w.Value.Equals(entry));
        public void SetConfig(List<Subscription> value)
        {
            Subscriptions.DeleteMany(_ => true);
            Subscriptions.InsertMany(Wrapper<Subscription>.WrapList(value));
        }
        public List<Subscription> GetConfig()
        {
            var list = Subscriptions.Find(_ => true).ToList();
            if (list is not null)
                return list.Select(w => w.Value).ToList();
            return new List<Subscription>();
        }
        #endregion
    }
}
