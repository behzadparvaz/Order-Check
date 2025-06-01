using OKEService.Core.ApplicationServices.Commands;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.ActiveItemRequestOrder
{
    public class ActiveItemRequestOrderCommand:ICommand
    {
        public string OrderCode { get; set; }
        public int? InsuranceTypeId { get; set; }
        public string? ReferenceNumber { get; set; }
        public List<ItemRequest>? Items { get; set; } = null;
    }

    public class ItemRequest
    {
        public string? IRC { get; set; }
        public string ProductName { get; set; }
        public string? ImageLink { get; set; }
        public string? AttachmentId { get; set; }
        public string? Unit { get; set; }
        public decimal Quantity { get; set; }
        public string? Description { get; set; }
        public int ProductType { get; set; } = 2;
    }
}
