using OKEService.Core.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TapsiDOC.Order.Core.Domain.Orders.Entities;

namespace TapsiDOC.Order.Core.Domain.Orders.DomainEvents
{
    public class AcceptEvent:IDomainEvent
    {
        public OrderStatus OrderStatus { get; }
        public string OrderCode { get; }
        public string VendorCode { get; }

        public AcceptEvent(OrderStatus orderStatus, string orderCode, string vendorCode)
        {
            OrderStatus = orderStatus;
            OrderCode = orderCode;
            VendorCode = vendorCode;
        }
    }
}
