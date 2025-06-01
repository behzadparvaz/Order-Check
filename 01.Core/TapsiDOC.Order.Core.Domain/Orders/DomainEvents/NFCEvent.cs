using OKEService.Core.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TapsiDOC.Order.Core.Domain.Orders.Entities;

namespace TapsiDOC.Order.Core.Domain.Orders.DomainEvents
{
    public class NFCEvent:IDomainEvent
    {
        public OrderStatus OrderStatus { get; }
        public string OrderCode { get; }
        public string PharmacyCode { get; }

        public NFCEvent(OrderStatus orderStatus, string orderCode, string pharmacyCode)
        {
            OrderStatus = orderStatus;
            OrderCode = orderCode;
            PharmacyCode = pharmacyCode;
        }
    }
}
