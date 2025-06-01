using OKEService.Core.ApplicationServices.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.UpdateOrderStatus
{
    public class UpdateOrderStatusCommand:ICommand
    {
        public string Id { get; set; }
        public string CreateDateTimeEvent { get; set; }
        public string? DateTimeRaiseEvent { get; set; }
        public string AggregateIdentifier { get; set; }
        public string AggregateType { get; set; }
        public int Version { get; set; }
        public string EventType { get; set; }
        public string OrderCode { get; set; }
        public int StatusId { get; set; }
        public string VendorCode { get; set; }
    }
}
