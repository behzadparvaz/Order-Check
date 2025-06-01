using OKEService.Core.Domain.Events;
using TapsiDOC.Order.Core.Domain.Orders.Entities;

namespace TapsiDOC.Order.Core.Domain.Orders.DomainEvents
{
    public class DraftEvent : IDomainEvent
    {
        public OrderStatus  OrderStatus { get; }
        public string OrderCode { get; }

        public DraftEvent(OrderStatus orderStatus ,  string orderCode)
        {
            OrderStatus = orderStatus;
            OrderCode = orderCode;
        }
    }
}
