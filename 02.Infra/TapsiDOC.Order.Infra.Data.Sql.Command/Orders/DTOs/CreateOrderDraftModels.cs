using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TapsiDOC.Order.Infra.Data.Sql.Commands.Orders.DTOs
{
    public class CreateOrderDraftModels
    {
        public string OrderCode { get; set; }
        public int InsuranceTypeId { get; set; }
        public int SupplementaryInsuranceTypeId { get; set; }
        public string? VendorCode { get; set; }
        public string Comment { get; set; }
        public string? CustomerName { get; set; }
        public string? NationalCode { get; set; }
        public string CreateDateTime { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsSpecialPatient { get; set; }
        public bool IsScheduled { get; set; }
        public List<Item> Items { get; set; } = null;
    }

    public class Item
    {
        public string IRC { get; set; }
        public string GTIN { get; set; }
        public string ProductName { get; set; }
        public decimal Quantity { get; set; }
        public string Description { get; set; }
        public string? ReferenceNumber { get; set; }
        public string Unit { get; set; }
        public string ImageLink { get; set; }
        public string? AttachmentId { get; set; }
        public int ProductType { get; set; }
    }
}
