using OKEService.Core.ApplicationServices.Commands;
using TapsiDOC.Order.Core.Domain.Contracts;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.VerifyPaymentOrder
{
    public class VerifyPaymentOrderCommand:ICommand<PaymentData>
    {
        public string TrackId { get; set; }
        public string OrderCode { get; set; }
    }
}
