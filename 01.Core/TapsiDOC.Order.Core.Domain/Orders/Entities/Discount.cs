using OKEService.Core.Domain.Entities;

namespace TapsiDOC.Order.Core.Domain.Orders.Entities
{
    public class Discount : Entity
    {
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
        public string? CouponCode { get; set; }
        public static Discount Create(decimal amount, decimal percentage)
        {
            //0,100
            return new()
            {
                Amount = amount,
                Percentage = percentage
            };
        }
        public void SetCouponCode(string? couponCode)
        {
            CouponCode = couponCode;
        }

    }
}
