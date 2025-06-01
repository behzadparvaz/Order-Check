using OKEService.Core.Domain.Events;
using TapsiDOC.Order.Core.Domain.Orders.Entities;

namespace TapsiDOC.Order.Core.Domain.Orders.DomainEvents
{
    public class AckEvent : IDomainEvent
    {
        public OrderStatus OrderStatus { get; }
        public string OrderCode { get; }
        public string PharmacyCode { get;  }

        public AckEvent(OrderStatus orderStatus, string orderCode, string pharmacyCode)
        {
            OrderStatus = orderStatus;
            OrderCode = orderCode;
            PharmacyCode = pharmacyCode;
        }
    }
}
