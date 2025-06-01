using MediatR;
using Microsoft.Extensions.Logging;
using OKEService.Core.ApplicationServices.Commands;
using OKEService.Utilities;
using System.Text.Json;
using TapsiDOC.Order.Core.Domain.Orders.Entities;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;
using MyEntities = TapsiDOC.Order.Core.Domain.Orders.Entities;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.UpdateOrderStatus
{
    public class UpdateOrderStatusCommandHandler : CommandHandler<UpdateOrderStatusCommand>
    {
        private readonly IOrderCommandRepository _orderCommandRepository;
        private readonly IOrderQueryRepository _orderQueryRepository;
        private readonly ILogger<UpdateOrderStatusCommandHandler> _logger;

        public UpdateOrderStatusCommandHandler(OKEServiceServices okeserviceServices,
            IOrderCommandRepository orderCommandRepository,
            IOrderQueryRepository orderQueryRepository,
            ILogger<UpdateOrderStatusCommandHandler> logger) : base(okeserviceServices)
        {
            _orderCommandRepository = orderCommandRepository;
            _orderQueryRepository = orderQueryRepository;
            _logger = logger;
        }

        public override async Task<CommandResult> Handle(UpdateOrderStatusCommand command)
        {
            _logger.LogDebug("AlternativeProductCommandHandler started");
            _logger.LogDebug("AlternativeProductCommandHandler request: {@command}", command);

            //if (!command.AggregateType.Equals("Delivery"))
            //    return default;

            var order = await _orderQueryRepository.FindOrderByVendor(command.OrderCode, command.VendorCode);
            if (order == null)
                throw new ArgumentException("شماره سفارش نامعتبر است");

            if (command.StatusId == MyEntities.DeliveryStatus.CancelBiker.Id)
                order.SetCancelBiker();
            else if (command.StatusId == MyEntities.DeliveryStatus.Pickup.Id)
                order.SetPickupDelivery();
            else
                order.SetDelivered();

            await _orderCommandRepository.UpdateStatus(order);

            _logger.LogDebug("AlternativeProductCommandHandler Done");

            return Ok();
        }
    }
}
