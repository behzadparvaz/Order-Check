

using OKEService.Core.ApplicationServices.Commands;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.UpdateDeliveryStatus
{
    public class UpdateDeliveryStatusCommand:ICommand
    {
        public string OrderCode { get; set; }
    }
}
