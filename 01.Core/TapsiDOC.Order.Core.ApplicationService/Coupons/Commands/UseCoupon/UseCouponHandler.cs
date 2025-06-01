using MediatR;
using Microsoft.Extensions.Logging;
using TapsiDOC.Order.Core.Domain.Coupons.Contracts.Repositories.SQL;
using TapsiDOC.Order.Core.Domain.Coupons.Entities;
using TapsiDOC.Order.Core.Domain.Coupons.Exceptions;
using TapsiDOC.Order.Core.Domain.Orders.Entities;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;

using OrderEntity = TapsiDOC.Order.Core.Domain.Orders.Entities.Order;

namespace TapsiDOC.Order.Core.ApplicationService.Coupons.Commands.UseCoupon
{
    public class UseCouponHandler : IRequestHandler<UseCouponCommand, UseCouponDto>
    {
        private readonly ICouponQueryRepository _couponQueryRepository;
        private readonly ICouponCommandRepository _couponCommandRepository;
        private readonly IOrderQueryRepository _orderQueryRepository;
        private readonly IOrderCommandRepository _orderCommandRepository;
        private readonly ILogger<UseCouponHandler> _logger;
        public const string _FreeOrderCode = "001TpaisDr";
        public UseCouponHandler(ICouponCommandRepository couponCommandRepository, ICouponQueryRepository couponQueryRepository, IOrderQueryRepository orderQueryRepository, IOrderCommandRepository orderCommandRepository, ILogger<UseCouponHandler> logger)
        {
            _couponCommandRepository = couponCommandRepository;
            _couponQueryRepository = couponQueryRepository;
            _orderQueryRepository = orderQueryRepository;
            _orderCommandRepository = orderCommandRepository;
            _logger = logger;
        }

        public async Task<UseCouponDto> Handle(UseCouponCommand request, CancellationToken cancellationToken)
        {
            string trackId = Guid.NewGuid().ToString("N");
            var result = new UseCouponDto();
            try
            {
                _logger.LogInformation("Use coupon,\ttrackId: {trackId},\torderCode: {orderCode},\tcouponCode:{couponCode}",
                    trackId,
                    request.OrderCode,
                    request.CouponCode);

                var orders = await _orderQueryRepository.GetOrder(request.OrderCode);
                if (orders == null || !orders.Any())
                    throw new NotFoundException("سفارش یافت نشد.");

                var couponInfo = await _couponQueryRepository.GetCouponByCode(request.CouponCode, cancellationToken)
                    ?? throw new NotFoundException("کدتخفیف یافت نشد.");

                if (couponInfo.ExpireAt.HasValue && couponInfo.ExpireAt < DateTime.Now)
                    throw new NotFoundException("کدتخفیف منقضی شده است.");

                if (couponInfo.Code != _FreeOrderCode)
                    //check user can use coupon
                    await CanUseCoupon(couponInfo.Id, request.OrderCode, request.UserId, cancellationToken);

                //set free delivery price
                await FreeDelivery(orders, request.CouponCode);
                //add history for user coupon use
                await AddCouponUse(couponInfo.Id, request.OrderCode, request.UserId, cancellationToken);

                result.Message = "با موفقیت ثبت شد.";
                result.IsSuccess = true;
            }
            catch (BaseCouponException ex)
            {
                result.Message = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Use coupon excetion,\ttrackId: {trackId},\tmessage: {msg},\torderCode: {orderCode},\tcouponCode: {couponCode}",
                    trackId,
                    ex.Message,
                    request.OrderCode,
                    request.CouponCode);

                result.Message = "خطای رخ داده لطفا مجدد تلاش نمایید";
            }
            return result;
        }
        private async Task CanUseCoupon(long couponId, string requestOrderCode, long userId, CancellationToken cancellationToken = default)
        {
            var couponUses = await _couponQueryRepository.GetCouponUses(userId, couponId, cancellationToken);
            if (couponUses.Any())
            {
                foreach (var couponUse in couponUses)
                {
                    var orderInNotValidState = await _orderQueryRepository.GetOrderForCoupon(
                        [
                            OrderStatus.Pick.Id,
                            OrderStatus.NFC.Id,
                            OrderStatus.Accept.Id,
                            OrderStatus.ADelivery.Id,
                            OrderStatus.Deliverd.Id,
                        ], couponUse.OrderCode);

                    if (orderInNotValidState.Any())
                        throw new BadRequestException($"این کدتخفیف برای سفارش: {orderInNotValidState[0].OrderCode} استفاده شده است.");

                    if (couponUse.IsDeleted == false)
                    {
                        if (couponUse.OrderCode == requestOrderCode)
                            throw new BadRequestException($"درخواست تکراری - این کد قبلا استفاده شده است.");

                        var isRejectOrder = await _orderQueryRepository.GetOrderForCoupon([
                            OrderStatus.CancelCustomer.Id,
                            OrderStatus.Reject.Id,
                        ], couponUse.OrderCode);

                        if (isRejectOrder.Any())
                            continue;

                        throw new BadRequestException($"این کدتخفیف برای سفارش: {couponUse.OrderCode} استفاده شده است..");
                    }
                }
            }
        }
        private async Task FreeDelivery(List<OrderEntity> orders, string couponCode)
        {
            bool isNeedUpdatePrice = false;
            foreach (var order in orders)
            {
                if (order.Delivery.FinalPrice <= 0)
                    continue;

                if (couponCode == _FreeOrderCode)
                {
                    order.Delivery.FinalPrice = 0;
                    order.TotalPrice = 0;
                    order.PackingPrice = 0;
                    order.SupervisorPharmacyPrice = 0;
                }

                order.SetDeliveryPrice(order.Delivery.FinalPrice, order.Delivery.FinalPrice);
                order.SetPrice(order.TotalPrice, order.PackingPrice, order.SupervisorPharmacyPrice);
                order.Delivery.Discount.SetCouponCode(couponCode);

                isNeedUpdatePrice = true;
                await _orderCommandRepository.UpdateDelivery(order);
            }
            if (isNeedUpdatePrice == false)
                throw new BadRequestException($"ارسال رایگان است؛ نیازی به کد تخفیف نیست.");

            //TODO: Update range 
        }
        private async Task<CouponUse> AddCouponUse(long couponId, string orderCode, long userId, CancellationToken cancellationToken = default)
        {
            var couponUse = CouponUse.Create(couponId, userId, orderCode);
            couponUse = await _couponCommandRepository.AddCouponUse(couponUse, cancellationToken);
            return couponUse;
        }

    }
}
