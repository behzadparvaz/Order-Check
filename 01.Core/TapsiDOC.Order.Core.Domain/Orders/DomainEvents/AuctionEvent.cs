using OKEService.Core.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TapsiDOC.Order.Core.Domain.Orders.Entities;

namespace TapsiDOC.Order.Core.Domain.Orders.DomainEvents
{
    public class AuctionEvent:IDomainEvent
    {
        public OrderStatus OrderStatus { get; }
        public decimal PaymentAmount { get; }
        public string OrderCode { get; }
        public string VendorCode { get; }

        public AuctionEvent(OrderStatus orderStatus, string orderCode, string vendorCode, decimal paymentAmount)
        {
            OrderStatus = orderStatus;
            OrderCode = orderCode;
            VendorCode = vendorCode;
            PaymentAmount = paymentAmount;
        }
    }
}
