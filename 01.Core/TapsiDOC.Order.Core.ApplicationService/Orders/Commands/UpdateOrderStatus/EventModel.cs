namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.UpdateOrderStatus
{
    internal class EventModel
    {
        public Vendor Vendor { get; set; }
        public Customer Customer { get; set; }
        public Order Order { get; set; }
        public DeliveryStatus DeliveryStatus { get; set; }

    }
    internal class Vendor
    {
        public string Name { get; set; }
        public string Code { get; set; }
    }

    internal class Customer
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
    }

    internal class Order
    {
        public string OrderCode { get; set; }

    }

    internal class DeliveryStatus
    {
        public string Name { get; set; }
        public int Id { get; set; }
    }
}
