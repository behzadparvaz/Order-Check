using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TapsiDOC.Order.Core.Domain.Orders.Entities;

namespace TapsiDOC.Order.Core.Domain.Orders.Repositories
{
    public interface IEventStoreRepository
    {
        Task SaveAysnc(EventData eventData);
        Task ProduceAsync<T>(string topic, T @event) where T : EventData;
        Task<List<EventData>> FindByAggregateId(string aggregateid);
        Task UpdateRaiseEvent(EventData eventData);

    }
}
