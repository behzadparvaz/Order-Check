using Microsoft.Extensions.Logging;
using OKEService.Core.ApplicationServices.Commands;
using OKEService.Utilities;
using TapsiDOC.Order.Core.Domain.Contracts;
using TapsiDOC.Order.Core.Domain.Orders.Entities;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.ActiveItemRequestOrder
{
    public class ActiveItemRequestOrderCommandHandler : CommandHandler<ActiveItemRequestOrderCommand>
    {
        private readonly IOrderCommandRepository command;
        private readonly IOrderSmsSender smsSender;
        private readonly IOMSSender oMSSender;
        private readonly IOrderQueryRepository query;
        private readonly ILogger<ActiveItemRequestOrderCommandHandler> logger;

        public ActiveItemRequestOrderCommandHandler(OKEServiceServices okeserviceServices,
            IOrderCommandRepository command,
            IOrderSmsSender smsSender,
            IOMSSender oMSSender,
            IOrderQueryRepository query,
            ILogger<ActiveItemRequestOrderCommandHandler> logger) : base(okeserviceServices)
        {
            this.command = command;
            this.smsSender = smsSender;
            this.oMSSender = oMSSender;
            this.query = query;
            this.logger = logger;
        }

        public override async Task<CommandResult> Handle(ActiveItemRequestOrderCommand request)
        {
            logger.LogDebug("Start ActiveItemRequestOrderCommandHandler");

            logger.LogDebug("ActiveItemRequestOrderCommandHandler Input request:{@request}", request);

            var order = await this.query.GetOrder(request.OrderCode);
            var orderFirst = order.FirstOrDefault();
            orderFirst.SetAckStatus();
            orderFirst.OrderDetails.RemoveAll(a => a.Type.Id == OrderDetailType.RequestOrder.Id);
            if (request.Items != null)
            {
                if (!string.IsNullOrEmpty(request.ReferenceNumber))
                {
                    orderFirst.OrderDetails.Add(
                                     OrderDetail.CreateReferenceNumber(request.ReferenceNumber));
                }
                foreach (var item in request.Items)
                {
                    item.ProductType = 2;
                    orderFirst.OrderDetails.Add(
                            OrderDetail.Create(item.IRC, string.Empty, item.ProductName, 0, 0, 0, item.Quantity, item.ImageLink,
                                                item.ProductType, item.Unit, item.Description, item.AttachmentId));
                }


            }
            foreach (var item in order)
            {
                item.OrderDetails = orderFirst.OrderDetails;
                item.InsuranceType = InsuranceType.From(request.InsuranceTypeId == null ? 0 : request.InsuranceTypeId);
                await this.oMSSender.SendOms(item);

                logger.LogDebug("ActiveItemRequestOrderCommandHandler oMSSender SendOms Done");

            }
            await this.command.UpdateOrderDetails(orderFirst);
            await this.smsSender.SendNotification(orderFirst);

            logger.LogDebug("ActiveItemRequestOrderCommandHandler Done");

            return Ok();
        }
    }
}
