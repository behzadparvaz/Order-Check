using System;
using System.ComponentModel.DataAnnotations;

namespace TapsiDOC.Order.Core.Domain.Coupons.Entities
{
    public class Coupon
    {
        public long Id { get; set; }
        [MaxLength(10)]
        public string Code { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpireAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
