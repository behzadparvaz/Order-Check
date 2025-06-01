using OKEService.Core.ApplicationServices.Commands;
using TapsiDOC.Order.Core.Domain.Orders.Entities;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.DoPayment
{
    public class DoPaymentCommand : ICommand
    {
        public string Id { get;  set; }
        public string CreateDateTimeEvent { get;  set; }
        public string? DateTimeRaiseEvent { get;  set; }
        public string AggregateIdentifier { get;  set; }
        public string AggregateType { get;  set; }
        public int Version { get;  set; }
        public string EventType { get;  set; }
        public string Event { get;  set; }
    }
}
