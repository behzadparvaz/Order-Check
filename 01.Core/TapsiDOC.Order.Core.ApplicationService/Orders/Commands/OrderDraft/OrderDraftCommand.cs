using MediatR;
using OKEService.Core.ApplicationServices.Commands;
using System.ComponentModel.DataAnnotations;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.OrderDraft
{
    public class OrderDraftCommand : IRequest<string>
    {
        public string? orderCode { get; set; }
        public int InsuranceTypeId { get; set; }
        public int SupplementaryInsuranceTypeId { get; set; }
        public bool IsSpecialPatient { get; set; }
        public string? VendorCode { get; set; }
        public string? Comment { get; set; }
        public string? PhoneNumber { get; set; }
        public string? NationalCode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? CustomerName { get; set; }
        public string? ValueAddress { get; set; }
        public string? TitleAddress { get; set; }
        public string? HouseNumber { get; set; }
        public int HomeUnit { get; set; }
        public string? DeliveryDate { get; set; }
        public string? FromDeliveryTime { get; set; }
        public string? ToDeliveryTime { get; set; }
        public string? PrepartionTime { get; set; }
        public List<Item>? Items { get; set; } = null;
        public string? Token { get; set; }
        public string? AlternateRecipientName { get; set; }
        public string? AlternateRecipientMobileNumber { get; set; }
    }
    public class Item
    {
        public string? IRC { get; set; }
        public string? GTIN { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? ProductName { get; set; }
        public string? ImageLink { get; set; }
        public string? AttachmentId { get; set; }
        public string? Unit { get; set; }
        public decimal Quantity { get; set; }
        public string? Description { get; set; }
        public int ProductType { get; set; }
    }
}
