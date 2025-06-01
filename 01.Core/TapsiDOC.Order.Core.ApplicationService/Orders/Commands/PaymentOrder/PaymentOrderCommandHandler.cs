using Microsoft.Extensions.Logging;
using OKEService.Core.ApplicationServices.Commands;
using OKEService.Utilities;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.PaymentOrder
{
    public class PaymentOrderCommandHandler : CommandHandler<PaymentOrderCommand, string>
    {
        private readonly IOrderCommandRepository commandRepository;
        private readonly IOrderQueryRepository queryRepository;
        private readonly ILogger<PaymentOrderCommandHandler> logger;

        public PaymentOrderCommandHandler(OKEServiceServices okeserviceServices,
            IOrderCommandRepository commandRepository,
            IOrderQueryRepository queryRepository,
            ILogger<PaymentOrderCommandHandler> logger) : base(okeserviceServices)
        {
            this.commandRepository = commandRepository;
            this.queryRepository = queryRepository;
            this.logger = logger;
        }

        public async override Task<CommandResult<string>> Handle(PaymentOrderCommand command)
        {
            logger.LogDebug("PaymentOrderCommandHandler started");
            logger.LogDebug("PaymentOrderCommandHandler command: {@command}", command);
            string[] royals = { "V000164", "V000170" };
            var order = await queryRepository.FindOrderByVendor(command.OrderCode, command.VendorCode);
            if (order == null)
                throw new Exception("Order not found!");


            if (royals.Contains(command.VendorCode) || command.IsSchedule)//if (command.VendorCode == "V00051")
            {
                order.CalDeliveryPriceRoyal(command.IsSchedule);
                if ((royals.Contains(command.VendorCode) && !string.IsNullOrEmpty(command.DeliveryDate)) || command.IsSchedule) //if (command.VendorCode == "V00051" && !string.IsNullOrEmpty(command.DeliveryDate))
                {
                    string FromDeliveryTime = string.Empty;
                    string ToDeliveryTime = string.Empty;
                    if (command.DeliveryTimeId == 1)
                    {
                        FromDeliveryTime = "09:00";
                        ToDeliveryTime = "15:00";
                    }
                    else
                    {
                        FromDeliveryTime = "15:00";
                        ToDeliveryTime = "21:00";
                    }
                    order.SetDelivery(command.DeliveryDate, FromDeliveryTime, ToDeliveryTime);
                }
                await this.commandRepository.UpdateDeliveryRoyal(order);
            }

            if (command.FinalPrice != order.FinalPrice)
                throw new ArgumentException("مبلغ پرداختی نادرست است");

            //// add chrto pert dolati

            if (order.FinalPrice == 0)
            {
                order.SetPickStatus();
                order.SetPickState();
                await commandRepository.UpdatePickOrder(order);

                logger.LogDebug("PaymentOrderCommandHandler with FinalPrice=0 Done");

                return Ok("orders-history");
            }

            var result = await this.commandRepository.Payment(command.OrderCode, command.PhoneNumber, order.FinalPrice, command.VendorCode);

            logger.LogDebug("PaymentOrderCommandHandler result: {result}", result);

            return Ok(result);
        }
    }
}
