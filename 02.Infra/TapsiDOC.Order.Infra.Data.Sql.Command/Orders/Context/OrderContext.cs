using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TapsiDOC.Order.Core.Domain.Orders.Entities;
using TapsiDOC.Order.Infra.Data.Sql.Commands.Orders.ContextSetting;
using MyEntities = TapsiDOC.Order.Core.Domain.Orders.Entities;

namespace TapsiDOC.Order.Infra.Data.Sql.Commands.Orders.Context
{
    public class OrderContext
    {
        private readonly IMongoDatabase _database = null;
        private readonly IMongoClient client;

        public OrderContext(IOptions<Settings> settings)
        {
            //var mongoClientSettings = MongoClientSettings.FromConnectionString(settings.Value.ConnectionString);

            //mongoClientSettings.RetryWrites = true;
            //mongoClientSettings.WaitQueueTimeout = TimeSpan.FromSeconds(5);

            client = new MongoClient(settings.Value.ConnectionString);

            if (client != null)
                _database = client.GetDatabase(settings.Value.Database);
        }

        public IMongoCollection<MyEntities.Order> OrdersCollection
        {
            get
            {
                return _database.GetCollection<MyEntities.Order>("Orders");
            }
        }


        public IMongoCollection<EventData> Events
        {
            get
            {
                return _database.GetCollection<EventData>("Events");
            }
        }

        public IClientSessionHandle StartSession()
        {
            return client.StartSession();
        }
    }
}
