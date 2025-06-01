using Confluent.Kafka;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;
using TapsiDOC.Order.Core.Domain.Orders.Entities;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;
using TapsiDOC.Order.Infra.Event.Kafka.Context;
using TapsiDOC.Order.Infra.Event.Kafka.ContextSettingEvent;

namespace TapsiDOC.Order.Infra.Event.Kafka.Orders
{
    public class EventStoreRepository : IEventStoreRepository
    {
        private readonly ProducerConfig _config;
        private readonly IOptions<Settings> _settings;
        private readonly EventContext _context = null;
        public EventStoreRepository(IOptions<ProducerConfig> config , IOptions<Settings> settings)
        {
            _config = config.Value;
            _context = new EventContext(settings);
            _settings = settings;
        }

        public async Task<List<EventData>> FindByAggregateId(string aggregateid)
        {
            var filter = Builders<EventData>.Filter.Eq("AggregateIdentifier", aggregateid);
            return await _context.Events.FindAsync(filter).Result.ToListAsync().ConfigureAwait(false);
        }

        public async Task ProduceAsync<T>(string topic, T @event) where T : EventData
        {
            using var producer = new ProducerBuilder<string, string>(_config)
                            .SetKeySerializer(Serializers.Utf8)
                            .SetValueSerializer(Serializers.Utf8)
                            .Build();

            var eventMessage = new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = JsonSerializer.Serialize(@event, @event.GetType())
            };

            var deliveryResult = await producer.ProduceAsync(topic, eventMessage);

            if (deliveryResult.Status == PersistenceStatus.NotPersisted)
            {
                throw new Exception($"Could not produce {@event.GetType().Name} message to topic - {topic} due to the following reason: {deliveryResult.Message}.");
            }
        }

        public async Task SaveAysnc(EventData eventData)
        {
            await _context.Events.InsertOneAsync(eventData).ConfigureAwait(false);
        }

        public async Task UpdateRaiseEvent(EventData eventData)
        {
            var filter = Builders<EventData>.Filter.Eq("AggregateIdentifier", eventData.AggregateIdentifier);
            var update = Builders<EventData>.Update.Set("DateTimeRaiseEvent", eventData.DateTimeRaiseEvent);
            await _context.Events.UpdateOneAsync(filter, update);
        }
    }
}
