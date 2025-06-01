using MediatR;
using TapsiDOC.Order.Core.Domain.Coupons.Contracts.Repositories.SQL;
using TapsiDOC.Order.Core.Domain.Coupons.Exceptions;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;

namespace TapsiDOC.Order.Core.ApplicationService.Coupons.Commands.DeleteCoupon
{
    public class DeleteCouponHandler : IRequestHandler<DeleteCouponCommand, DeleteCouponDto>
    {
        private readonly ICouponCommandRepository _couponCommandRepository;
        private readonly ICouponQueryRepository _couponQueryRepository;
        private readonly IOrderQueryRepository _orderQueryRepository;
        private readonly IOrderCommandRepository _orderCommandRepository;
        public DeleteCouponHandler(ICouponCommandRepository couponCommandRepository, ICouponQueryRepository couponQueryRepository, IOrderQueryRepository orderQueryRepository, IOrderCommandRepository orderCommandRepository)
        {
            _couponCommandRepository = couponCommandRepository;
            _couponQueryRepository = couponQueryRepository;
            _orderQueryRepository = orderQueryRepository;
            _orderCommandRepository = orderCommandRepository;
        }

        public async Task<DeleteCouponDto> Handle(DeleteCouponCommand request, CancellationToken cancellationToken)
        {
            var result = new DeleteCouponDto();
            var couponInfo = await _couponQueryRepository.GetCouponByCode(request.CouponCode, cancellationToken)
                ?? throw new NotFoundException("اطلاعات کوپن یافت نشد.");

            var allCouponUses = await _couponQueryRepository.GetCouponUses(request.UserId, couponInfo.Id, cancellationToken);
            if (!allCouponUses.Any())
                return result;

            var orders = await _orderQueryRepository.GetOrder(request.OrderCode);
            foreach (var order in orders)
            {
                var deliveryPrice = await _orderQueryRepository.GetDeliveryPrice(order);
                order.SetDeliveryPrice(deliveryPrice, 0);
                order.SetPrice(order.TotalPrice, order.PackingPrice, order.SupervisorPharmacyPrice);
                if (order.Discount != null)
                    order.Discount.SetCouponCode(null);
                await _orderCommandRepository.UpdateDelivery(order);
            }

            var orderCouponUses = allCouponUses.Where(z => z.OrderCode == request.OrderCode).ToList();
            foreach (var couponUse in orderCouponUses)
            {
                couponUse.SetIsDelete();
            }
            result.IsSuccess = await _couponCommandRepository.UpdateRangeCouponUses(allCouponUses, cancellationToken);
            return result;

        }
    }
}
