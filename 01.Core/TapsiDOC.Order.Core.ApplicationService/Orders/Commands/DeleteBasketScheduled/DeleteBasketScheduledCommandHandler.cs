using MediatR;
using Microsoft.Extensions.Logging;
using TapsiDOC.Order.Core.ApplicationService.Orders.Commands.DeleteBasket;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.DeleteBasketScheduled
{
    public class DeleteBasketScheduledCommandHandler : IRequestHandler<DeleteBasketScheduledCommand>
    {

        private readonly ILogger<DeleteBasketScheduledCommandHandler> logger;
        private readonly IOutboxQueryRepository outboxQuery;
        private readonly IOrderCommandRepository orderCommand;
        private readonly IOrderQueryRepository orderQuery;

        public DeleteBasketScheduledCommandHandler(IOutboxQueryRepository outboxQuery,
            IOrderCommandRepository orderCommand,
            IOrderQueryRepository orderQuery,
            ILogger<DeleteBasketScheduledCommandHandler> logger) 
        {
            this.logger = logger;
            this.outboxQuery = outboxQuery;
            this.orderCommand = orderCommand;
            this.orderQuery = orderQuery;
        }

        public async Task<Unit> Handle(DeleteBasketScheduledCommand request,
            CancellationToken cancellationToken)
        {
            logger.LogDebug("DeleteBasketScheduledCommand started");

            var outboxItems = await outboxQuery.GetOutBoxItemsByAggregate("Basket");

            foreach (var outbox in outboxItems)
            {
                var order = await this.orderQuery.GetOrderByOrderCode(outbox.Json);

                logger.LogDebug("DeleteBasketScheduledCommand before raising event for outbox: {@recId}", outbox.RecId);

                await orderCommand.RaiseEventDeleteBasketScheduler(order, outbox.Token, outbox.RecId);

                logger.LogDebug("DeleteBasketScheduledCommand raised event for outbox: {@recId}", outbox.RecId);
            }

            logger.LogDebug("DeleteBasketScheduledCommand Done");

            return Unit.Value;
        }
    }
}
