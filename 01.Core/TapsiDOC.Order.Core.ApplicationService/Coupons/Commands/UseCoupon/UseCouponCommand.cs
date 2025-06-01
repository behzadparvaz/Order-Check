using MediatR;

namespace TapsiDOC.Order.Core.ApplicationService.Coupons.Commands.UseCoupon
{
    public class UseCouponCommand : IRequest<UseCouponDto>
    {
        public long UserId { get; set; }
        public required string OrderCode { get; set; }
        public required string CouponCode { get; set; }
    }
}
