using MediatR;

namespace TapsiDOC.Order.Core.ApplicationService.Coupons.Commands.DeleteCoupon
{
    public class DeleteCouponCommand : IRequest<DeleteCouponDto>
    {
        public long UserId { get; set; }
        public required string CouponCode { get; set; }
        public required string OrderCode { get; set; }
    }
}
