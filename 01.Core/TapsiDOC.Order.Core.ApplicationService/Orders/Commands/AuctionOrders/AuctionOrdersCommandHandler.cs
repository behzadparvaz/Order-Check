using MediatR;
using Microsoft.Extensions.Logging;
using TapsiDOC.Order.Core.Domain.Contracts;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.AuctionOrders
{
    public class AuctionOrdersCommandHandler : IRequestHandler<AuctionOrdersCommand>
    {
        private readonly IOrderCommandRepository command;
        private readonly IOrderQueryRepository query;
        private readonly IOrderSmsSender smsSender;
        private readonly ILogger<AuctionOrdersCommandHandler> logger;

        public AuctionOrdersCommandHandler(IOrderCommandRepository command,
            IOrderQueryRepository query,
            IOrderSmsSender smsSender,
            ILogger<AuctionOrdersCommandHandler> logger)
        {
            this.command = command;
            this.query = query;
            this.smsSender = smsSender;
            this.logger = logger;
        }

        public async Task<Unit> Handle(AuctionOrdersCommand request, CancellationToken cancellationToken)
        {
            var orders = await this.query.FetchAuctionOrder();
            if (orders != null)
            {
                if (orders.Count > 0)
                {
                    foreach (var order in orders)
                    {
                        var orderRoyall = await this.query.FindOrderByVendorReyall(order.OrderCode, "V00051");
                        if (orderRoyall != null)
                        {
                            orderRoyall.SetAPayStatus();
                            orderRoyall.SetAPayState();
                            await this.command.ChangeStateOrderApay(orderRoyall);
                        }
                        order.SetAPayStatus();
                        order.SetAPayState();
                        await this.command.ChangeStateOrderApay(order);
                        var check = await this.query.GetOrderAPay(order.OrderCode);
                        if (check.Count == 1)
                            await smsSender.SendNotification(order);
                    }
                }
            }
            return Unit.Value;
        }
    }
}
