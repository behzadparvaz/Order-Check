
using Microsoft.Extensions.Logging;
using OKEService.Core.ApplicationServices.Commands;
using OKEService.Utilities;
using System.Reflection.Metadata;
using System.Text.Json;
using TapsiDOC.Order.Core.Domain.Contracts;
using TapsiDOC.Order.Core.Domain.Orders.Entities;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;
using MyEntities = TapsiDOC.Order.Core.Domain.Orders.Entities;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.DoPayment
{
    public class DoPaymentCommandHandler : CommandHandler<DoPaymentCommand>
    {
        private readonly IOrderCommandRepository commandRepository;
        private readonly IOrderQueryRepository queryRepository;
        private readonly IOrderSmsSender smsSender;
        private readonly ILogger<DoPaymentCommandHandler> logger;

        public DoPaymentCommandHandler(OKEServiceServices okeserviceServices,
            IOrderCommandRepository commandRepository,
            IOrderQueryRepository queryRepository,
            IOrderSmsSender smsSender,
            ILogger<DoPaymentCommandHandler> logger) : base(okeserviceServices)
        {
            this.commandRepository = commandRepository;
            this.queryRepository = queryRepository;
            this.smsSender = smsSender;
            this.logger = logger;
        }


        public async override Task<CommandResult> Handle(DoPaymentCommand command)
        {
            logger.LogDebug("DoPaymentCommandHandler started");
            logger.LogDebug("DoPaymentCommandHandler input command: {@command}", command);

            if (command.AggregateType.Equals("OMS"))
            {
                var order = JsonSerializer.Deserialize<MyEntities.Order>(command.Event);
                var result = await this.queryRepository.FindOrderByVendor(order.OrderCode, order.VendorCode);
                if (result == null)
                    throw new ArgumentException("شماره سفارش نامعتبر است");

                //result.SetAuctionStatus();
                //result.SetAuctionState();
                result.SetAPayStatus();
                result.SetAPayState();
                //result.SetDeliveryPrice(0);//order.SetDeliveryPrice(await this.queryRepository.GetDeliveryPrice(result));
               // //var firstOrderByCustomer = await this.queryRepository.FindOrderByPhoneNumber(result.Customer.PhoneNumber);
                var deliveryPrice = await this.queryRepository.GetDeliveryPrice(result);
                decimal deliveryDiscount = 0;
                string[] royals = { "V000164", "V000170" };
               // //if (firstOrderByCustomer.Count == 0 || result.Delivery.IsScheduled || royals.Contains(order.VendorCode))
                deliveryDiscount = deliveryPrice;

                result.SetDeliveryPrice(deliveryPrice, deliveryDiscount);
                result.SetPrice(order.TotalPrice, order.PackingPrice, order.SupervisorPharmacyPrice);

                if (order.VendorComment != null)
                    result.SetVendorComment(order.VendorComment);

                foreach (var item in order.OrderDetails)
                {
                    result.OrderDetails.Find(a => a.IRC == item.IRC).SetPriceItem(item).SetExpirationDate(item.ExpirationDate);

                    if (item.DoctorInstruction != null)
                        result.OrderDetails.Find(a => a.IRC == item.IRC)?.SetDoctorInstruction(item.DoctorInstruction);
                }

                await this.commandRepository.UpdateStatus(result);
                await this.commandRepository.SaveEventsAsync(result.Id, result, result.OrderStatus.Id);
                var check = await this.queryRepository.GetOrderAPay(order.OrderCode);
                if (check.Count == 1)
                    await smsSender.SendNotification(result);
            }

            logger.LogDebug("DoPaymentCommandHandler Done");

            return Ok();
        }
    }
}
