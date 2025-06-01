using OKEService.Core.ApplicationServices.Commands;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.CancelOrder
{
    public class CancelOrderCommand : ICommand
    {
        public string? PhoneNumber { get; set; }
        public string OrderCode { get; set; }
        public int DeclineType { get; set; } = 18;
        public string? Reason { get; set; }
    }
}
