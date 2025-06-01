using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using OKEService.Core.ApplicationServices.Commands;
using OKEService.Utilities;
using TapsiDOC.Order.Core.Domain.Orders.Entities;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.CancelOrder
{
    public class CancelOrderCommandHandler : CommandHandler<CancelOrderCommand>
    {
        private readonly IOrderCommandRepository commandRepo;
        private readonly IOrderQueryRepository queryRepo;
        private readonly ILogger<CancelOrderCommandHandler> logger;

        public CancelOrderCommandHandler(OKEServiceServices okeserviceServices,
            IOrderCommandRepository commandRepo,
            IOrderQueryRepository queryRepo,
            ILogger<CancelOrderCommandHandler> logger) : base(okeserviceServices)
        {
            this.commandRepo = commandRepo;
            this.queryRepo = queryRepo;
            this.logger = logger;
        }

        public override async Task<CommandResult> Handle(CancelOrderCommand command)
        {
            logger.LogDebug("CancelOrderCommandHandler started");
            logger.LogDebug("CancelOrderCommandHandler input command: {@command}", command);

            var order = await queryRepo.GetOrderByOrderCode(command.OrderCode);
            logger.LogDebug("CancelOrderCommandHandler fetch Order  GetOrderByOrderCode: {@Order}", 
                                    JsonConvert.SerializeObject(order, new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() }));
            if (string.IsNullOrEmpty(order.Customer.PhoneNumber) || order.Customer.PhoneNumber != command.PhoneNumber)
                throw new ArgumentException("عدم دسترسی");
            if (order == null)
                throw new ArgumentException("سفارش مورد نظر یافت نشد!");
            if (command.DeclineType == DeclineType.OtherReason.Id)
                order.CancelReason = command.Reason;
            order.DeclineType = DeclineType.From(command.DeclineType);

            logger.LogDebug($"DeclineType: {order.DeclineType.Id} ,  OrderCode: {order.OrderCode}", 
                        JsonConvert.SerializeObject(order, new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() }));

            order.SetCancelCustomerStatus();
            order.SetCancelCustomerState();
            await commandRepo.UpdateCancelCustomer(order);

            logger.LogDebug("CancelOrderCommandHandler Done");

            return Ok();
        }
    }
}
