using OKEService.Core.Domain.Entities;

namespace TapsiDOC.Order.Core.Domain.Orders.Entities
{
    public class Alternative : Entity
    {
        public string ProductName { get; set; }
        public string BrandName { get; set; }
        public string IRC { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string Unit { get; set; }
        public string? Description { get; set; }
        public int ProductType { get; set; }
        public string ImageLink { get; set; }
        public ExpirationDate? ExpirationDate { get; set; }


        public static Alternative Create(string productName, string brandName, string irc, int quantity, decimal price, string unit, string description, int productType, string imageLink, ExpirationDate? expirationDate)
        {
            return new()
            {
                ProductName = productName,
                BrandName = brandName,
                IRC = irc,
                Quantity = quantity,
                Price = price,
                Unit = unit,
                Description = description,
                ProductType = productType,
                ImageLink = imageLink,
                ExpirationDate = expirationDate
            };
        }
    }
}
