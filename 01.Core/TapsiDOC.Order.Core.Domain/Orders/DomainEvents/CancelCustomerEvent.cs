using OKEService.Core.Domain.Events;
using TapsiDOC.Order.Core.Domain.Orders.Entities;

namespace TapsiDOC.Order.Core.Domain.Orders.DomainEvents
{
    public class CancelCustomerEvent : IDomainEvent
    {
        public OrderStatus OrderStatus { get; }
        public string OrderCode { get; }
        public string VendorCode { get; }

        public CancelCustomerEvent(OrderStatus orderStatus, string orderCode, string vendorCode)
        {
            OrderStatus = orderStatus;
            OrderCode = orderCode;
            VendorCode = vendorCode;
        }
    }
}
