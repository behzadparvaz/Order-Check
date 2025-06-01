using OKEService.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TapsiDOC.Order.Core.Domain.Orders.Entities
{
    public class EventData : Entity
    {
        public string Id { get; private set; }
        public string CreateDateTimeEvent { get; private set; }
        public string DateTimeRaiseEvent { get; private set; }
        public string AggregateIdentifier { get; private set; }
        public string AggregateType { get; private set; }
        public int Version { get; private set; }
        public string EventType { get; private set; }
        public string Event { get; private set; }

        public EventData Create(string aggregateIdentifier, string aggregateType, int version, string eventType, string eventData)
        {
            return new()
            {
                Id = Guid.NewGuid().ToString(),
                CreateDateTimeEvent = PersianDateTime(DateTime.Now),
                AggregateIdentifier = aggregateIdentifier,
                AggregateType = aggregateType,
                Version = version,
                EventType = eventType,
                Event = eventData
            };

        }

        public EventData SetRaiseDateTime(EventData eventData)
        {
            eventData.DateTimeRaiseEvent = PersianDateTime(DateTime.Now);
            return this;
        }

        private string PersianDateTime(DateTime date)
        {
            PersianCalendar p = new PersianCalendar();
            return string.Format(@"{0}/{01}/{02} {03}:{04}",
                p.GetYear(date),
                p.GetMonth(date),
                p.GetDayOfMonth(date),
                p.GetHour(date),
                p.GetMinute(date));
        }
    }
}
