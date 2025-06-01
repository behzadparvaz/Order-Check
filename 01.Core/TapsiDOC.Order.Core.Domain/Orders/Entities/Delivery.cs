using OKEService.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TapsiDOC.Order.Core.Domain.Orders.Entities
{
    public class Delivery : Entity
    {
        public decimal DeliveryPrice { get; set; }
        public decimal FinalPrice { get; set; }
        public Discount Discount { get; set; }
        public string DeliveryTime { get; set; }
        public Customer Customer { get; set; }
        public Order Order { get; set; }
        public Vendor Vendor { get; set; }
        public bool IsScheduled { get; set; }

        public DeliveryStatus DeliveryStatus { get; set; }

        public Delivery()
        {
            Discount = new Discount();
        }
        public Delivery Create(decimal deliveryPrice, string deliveryTime)
        {
            return new()
            {
                DeliveryPrice = deliveryPrice,
                DeliveryTime = deliveryTime
            };
        }

        public void SetDeliveryTime(string deliveryTime) =>
                                        DeliveryTime = deliveryTime;

        public void SetPrice(decimal price, decimal discount)
        {
            //0
            DeliveryPrice = price; //price + (decimal)((double)price * 0.1);
            Discount = Discount.Create(discount, 100);
            FinalPrice = DeliveryPrice - Discount.Amount;
        }


    }
}
