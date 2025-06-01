using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using TapsiDOC.Order.Infra.Data.Sql.Queries.Orders.ContextSetting;
using Aggregate = TapsiDOC.Order.Core.Domain.Orders.Entities;

namespace TapsiDOC.Order.Infra.Data.Sql.Queries.Orders.Context
{
    public class OrderContext
    {
        private readonly IMongoDatabase _database = null;

        public OrderContext(IOptions<Settings> settings)
        {
            //var mongoClientSettings = MongoClientSettings.FromConnectionString(settings.Value.ConnectionString);

            //mongoClientSettings.RetryWrites = true;
            //mongoClientSettings.WaitQueueTimeout = TimeSpan.FromSeconds(5);

            var client = new MongoClient(settings.Value.ConnectionString);

            if (client != null)
                _database = client.GetDatabase(settings.Value.Database);

            RegisterClassMap();
        }

        public IMongoCollection<Aggregate.Order> OrdersCollection
        {
            get
            {
                return _database.GetCollection<Aggregate.Order>("Orders");
            }
        }

        public IMongoCollection<BsonDocument> Collection
        {
            get
            {
                return _database.GetCollection<BsonDocument>("Orders");
            }
        }

        private void RegisterClassMap()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(Aggregate.Order)))
            {
                BsonClassMap.RegisterClassMap<Aggregate.Order>(cm =>
                {
                    cm.AutoMap();
                    cm.SetIgnoreExtraElements(true);
                });
            }
        }

        //public IMongoCollection<EventDto> EventsCollection
        //{
        //    get
        //    {
        //        return _database.GetCollection<EventDto>("Events");
        //    }
        //}
    }
}
