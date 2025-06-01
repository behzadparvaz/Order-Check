using OKEService.Core.ApplicationServices.Commands;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.RejectItemRequestOrder
{
    public class RejectItemRequestOrder:ICommand
    {
        public string OrderCode { get; set; }
        public string Description { get; set; }
    }
}
