using OKEService.Core.ApplicationServices.Commands;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.PaymentOrder
{
    public class PaymentOrderCommand : ICommand<string>
    {
        public string? PhoneNumber { get; set; }
        public string OrderCode { get; set; }
        public decimal FinalPrice { get; set; }
        public string VendorCode { get; set; }
        public string? DeliveryDate { get; set; }
        public int DeliveryTimeId { get; set; }
        public bool IsSchedule { get; set; }
    }
}
