using OKEService.Core.Domain.Entities;
using System.Collections.Generic;
using TapsiDOC.Order.Core.Domain.Orders.SeedWork;

namespace TapsiDOC.Order.Core.Domain.Orders.Entities
{
    public class OrderDetail : Entity
    {
        public string ERX { get; set; }
        public string IRC { get; set; }
        public string GTIN { get; set; }
        public string ReferenceNumber { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public Discount Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal Quantity { get; set; }
        public string ImageLink { get; set; }
        public string AttachmentId { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public OrderDetailType Type { get; set; }
        public OrderDetailStatus Status { get; set; }
        public bool Isunavailable { get; set; }
        public List<Alternative> Alternatives { get; set; }
        public string? DoctorInstruction { get; set; }
        public ExpirationDate? ExpirationDate { get; set; }
        public OrderDetail()
        {
            Alternatives = new List<Alternative>();
        }

        public void SetDoctorInstruction(string doctorInstruction) => DoctorInstruction = doctorInstruction;

        public void SetExpirationDate(ExpirationDate expirationDate) => ExpirationDate = expirationDate;


        public static OrderDetail Create(string IRC, string GTIN, string productName, decimal price, decimal discountAmount, decimal tax, decimal quantity, string imageLink, int type, string unit = "", string description = "", string attachmentId = "")
        {
            return new()
            {
                ERX = "",
                IRC = IRC,
                GTIN = GTIN,
                ProductName = productName,
                Price = price,
                Discount = Discount.Create(discountAmount, price),
                Tax = tax,
                Quantity = quantity,
                ImageLink = imageLink,
                Description = description,
                Status = OrderDetailStatus.Collect,
                Type = OrderDetailType.From(type),
                Unit = unit,
                AttachmentId = attachmentId
            };
        }

        public static OrderDetail CreateReferenceNumber(string referenceNumber)
        {
            if (!string.IsNullOrEmpty(referenceNumber))
                referenceNumber = referenceNumber.PersianToEnglish();
            return new()
            {
                ERX = string.Empty,
                IRC = string.Empty,
                GTIN = string.Empty,
                ReferenceNumber = referenceNumber,
                ProductName = string.Empty,
                Price = 0,
                Discount = Discount.Create(0, 0),
                Tax = 0,
                Quantity = 0,
                ImageLink = string.Empty,
                Description = string.Empty,
                Status = OrderDetailStatus.Collect,
                Type = OrderDetailType.RX
            };
        }

        public OrderDetail SetPriceItem(OrderDetail detail)
        {
            Price = detail.Price;
            Quantity = detail.Quantity;
            Isunavailable = detail.Isunavailable;
            Discount = Discount.Create(detail.Discount.Amount, detail.Discount.Percentage);
            foreach (var item in detail.Alternatives)
            {
                Alternatives.Add(Alternative.Create(item.ProductName, item.BrandName, item.IRC, item.Quantity, item.Price, item.Unit, item.Description, item.ProductType,
                                       item.ImageLink, item.ExpirationDate));
            }

            return this;
        }

    }
}
