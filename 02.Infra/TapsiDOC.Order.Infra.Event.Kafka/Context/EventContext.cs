using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TapsiDOC.Order.Core.Domain.Orders.Entities;
using TapsiDOC.Order.Infra.Event.Kafka.ContextSettingEvent;


namespace TapsiDOC.Order.Infra.Event.Kafka.Context
{
    public class EventContext
    {
        private readonly IMongoDatabase _database = null;

        public EventContext(IOptions<Settings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            if (client != null)
                _database = client.GetDatabase(settings.Value.Database);
        }

        public IMongoCollection<EventData> Events
        {
            get
            {
                return _database.GetCollection<EventData>("Events");
            }
        }
    }
}
