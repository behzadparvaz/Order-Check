using Microsoft.Extensions.Logging;
using OKEService.Core.ApplicationServices.Commands;
using OKEService.Utilities;
using System.Text.Json;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;
using MyEntities = TapsiDOC.Order.Core.Domain.Orders.Entities;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.AcceptOrder
{
    public class AcceptOrderCommandHandler : CommandHandler<AcceptOrderCommand>
    {
        private readonly IOrderCommandRepository command;
        private readonly IOrderQueryRepository query;
        private readonly ILogger<AcceptOrderCommandHandler> logger;

        public AcceptOrderCommandHandler(OKEServiceServices okeserviceServices,
            IOrderCommandRepository command,
            IOrderQueryRepository query,
            ILogger<AcceptOrderCommandHandler> logger) : base(okeserviceServices)
        {
            this.command = command;
            this.query = query;
            this.logger = logger;
        }

        public override async Task<CommandResult> Handle(AcceptOrderCommand request)
        {
            logger.LogDebug("AcceptOrderCommandHandler started");
            logger.LogDebug("AcceptOrderCommandHandler input request: {@request}", request);

            if (request.AggregateType.Equals("OMS"))
            {
                var result = JsonSerializer.Deserialize<MyEntities.Order>(request.Event);
                var order = await query.FindOrderByVendor(result.OrderCode, result.VendorCode);
                order.SetAWaitingDeliveryStatus();
                order.SetAWaitingDeliveryState();
                await command.UpdateStatus(order);

                logger.LogDebug("AcceptOrderCommandHandler Done");

                return Ok();
            }
            else
            {
                logger.LogDebug("AcceptOrderCommandHandler Done: aggregateType was not OMS");

                throw new ArgumentException("اطلاع رسانی با مشکل مواجه شد ");
            }
        }
    }
}
