using OKEService.Core.ApplicationServices.Commands;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.CancelOrderCallCenter
{
    public class CancelOrderCallCenterCommand : ICommand
    {
        public string? PhoneNumber { get; set; }
        public string OrderCode { get; set; }
        public int DeclineType { get; set; } = 30;
        public string? Reason { get; set; }
    }
}
