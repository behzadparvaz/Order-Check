using OKEService.Core.ApplicationServices.Commands;
using OKEService.Utilities;
using System.Text.Json;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;
using MyEntities = TapsiDOC.Order.Core.Domain.Orders.Entities;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.CancelVendorOrder
{
    public class CancelVendorOrderCommandHandler : CommandHandler<CancelVendorOrderCommand>
    {
        private readonly IOrderCommandRepository commandRepo;
        private readonly IOrderQueryRepository queryRepo;

        public CancelVendorOrderCommandHandler(OKEServiceServices okeserviceServices, IOrderCommandRepository commandRepo, IOrderQueryRepository queryRepo) : base(okeserviceServices)
        {
            this.commandRepo = commandRepo;
            this.queryRepo = queryRepo;
        }

        public override async Task<CommandResult> Handle(CancelVendorOrderCommand command)
        {
            if (command.AggregateType.Equals("OMS"))
            {
                if (!command.EventType.Equals("cancelvendor"))
                    throw new ArgumentException("ایونت ارسالی در حالت نامعتبری قرار دارد");
                var result = JsonSerializer.Deserialize<MyEntities.Order>(command.Event);
                var order = await queryRepo.GetOrderByOrderCode(result.OrderCode);
                order.VendorCode = result.VendorCode;
                if (order == null)
                    throw new ArgumentException("کد سفارش یافت نشد");
                order.SetCancelVendorStatus(result.DeclineType);
                order.SetCancelVendorState();
                await commandRepo.UpdateStatus(order);
                return Ok();

            }
            else
                throw new ArgumentException("دریافت اطلاع رسانی کنسلی توسط وندور با مشکل مواجه شد");
        }
    }
}
