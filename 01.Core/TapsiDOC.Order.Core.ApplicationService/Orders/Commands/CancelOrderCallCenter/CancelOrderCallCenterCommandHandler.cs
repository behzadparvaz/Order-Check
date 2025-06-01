using Microsoft.Extensions.Logging;
using OKEService.Core.ApplicationServices.Commands;
using OKEService.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TapsiDOC.Order.Core.Domain.Contracts;
using TapsiDOC.Order.Core.Domain.Orders.Entities;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.CancelOrderCallCenter
{
    public class CancelOrderCallCenterCommandHandler : CommandHandler<CancelOrderCallCenterCommand>
    {
        private readonly IOrderCommandRepository commandRepo;
        private readonly IOrderQueryRepository queryRepo;
        private readonly IOrderSmsSender smsSender;
        private readonly ILogger<CancelOrderCallCenterCommandHandler> logger;

        public CancelOrderCallCenterCommandHandler(OKEServiceServices okeserviceServices,
            IOrderCommandRepository commandRepo,
            IOrderQueryRepository queryRepo,
            IOrderSmsSender smsSender,
            ILogger<CancelOrderCallCenterCommandHandler> logger) : base(okeserviceServices)
        {
            this.smsSender = smsSender;
            this.commandRepo = commandRepo;
            this.queryRepo = queryRepo;
            this.logger = logger;
        }

        public override async Task<CommandResult> Handle(CancelOrderCallCenterCommand command)
        {
            logger.LogDebug("CancelOrderCallCenterCommandHandler started");
            logger.LogDebug("CancelOrderCallCenterCommandHandler command: {@command}", command);

            var order = await queryRepo.GetOrderByOrderCode(command.OrderCode);

            if (command.DeclineType == DeclineType.OtherReason.Id)
                order.CancelReason = command.Reason;

            order.DeclineType = DeclineType.From(command.DeclineType);
            order.SetRejectStatus();
            order.SetRejectState();

            if (order == null)
                throw new ArgumentException("سفارش مورد نظر یافت نشد!");

            await commandRepo.UpdateStatus(order);

            await smsSender.SendNotification(order);

            logger.LogDebug("CancelOrderCallCenterCommandHandler Done");

            return Ok();
        }
    }
}
