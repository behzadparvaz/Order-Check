using OKEService.Core.Domain.Events;
using TapsiDOC.Order.Core.Domain.Orders.Entities;

namespace TapsiDOC.Order.Core.Domain.Orders.DomainEvents
{
    public class ReceiveCancelVendorEvent : IDomainEvent
    {
        public OrderStatus OrderStatus { get; }
        public string OrderCode { get; }
        public string VendorCode { get; }
        public DeclineType DeclineType { get; }

        public ReceiveCancelVendorEvent(OrderStatus orderStatus, string orderCode, string vendorCode, DeclineType declineType)
        {
            OrderStatus = orderStatus;
            OrderCode = orderCode;
            VendorCode = vendorCode;
            DeclineType = declineType;
        }
    }
}
