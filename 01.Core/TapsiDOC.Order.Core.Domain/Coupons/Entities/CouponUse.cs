using System;
using System.ComponentModel.DataAnnotations;

namespace TapsiDOC.Order.Core.Domain.Coupons.Entities
{
    public class CouponUse
    {
        public long Id { get; set; }
        public long UserId { get; private set; }
        public long CouponId { get; private set; }
        [MaxLength(10)]
        public string OrderCode { get; private set; } = null!;
        public bool IsDeleted { get; private set; }
        public DateTime CreatedAt { get; private set; }
        [MaxLength(35)]
        public string Uuid { get; private set; } = null!;

        public static CouponUse Create(long couponId, long userId, string orderCode)
        {
            return new CouponUse()
            {
                CouponId = couponId,
                UserId = userId,
                OrderCode = orderCode,
                CreatedAt = DateTime.Now,
                Uuid = Guid.NewGuid().ToString("N").ToUpper()
            };
        }

        public void SetIsDelete()
        {
            IsDeleted = true;
        }
    }
}
