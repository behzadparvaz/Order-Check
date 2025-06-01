using OKEService.Core.Domain.Events;
using TapsiDOC.Order.Core.Domain.Orders.Entities;

namespace TapsiDOC.Order.Core.Domain.Orders.DomainEvents
{
    public class APayEvent : IDomainEvent
    {
        public OrderStatus OrderStatus { get; }
        public decimal PaymentAmount { get; }
        public string OrderCode { get; }
        public string VendorCode { get; }

        public APayEvent(OrderStatus orderStatus, string orderCode, string vendorCode, decimal paymentAmount)
        {
            OrderStatus = orderStatus;
            OrderCode = orderCode;
            VendorCode = vendorCode;
            PaymentAmount = paymentAmount;
        }
    }
}
