using OKEService.Core.ApplicationServices.Commands;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.CreateApproval
{
    public class CreateApprovalCommand : ICommand
    {
        public string? PhoneNumber { get; set; }
        public string? OrderCode { get; set; }
    }
}
