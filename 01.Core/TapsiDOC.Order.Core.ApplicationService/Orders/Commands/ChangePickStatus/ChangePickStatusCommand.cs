using OKEService.Core.ApplicationServices.Commands;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.ChangePickStatus
{
    public class ChangePickStatusCommand : ICommand
    {
        public string? PhoneNumber { get; set; }
        public string OrderCode { get; set; }
        public int StatusId { get; set; }
    }
}
