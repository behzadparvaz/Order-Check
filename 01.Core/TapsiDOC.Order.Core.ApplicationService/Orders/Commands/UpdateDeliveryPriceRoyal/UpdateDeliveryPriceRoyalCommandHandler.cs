using OKEService.Core.ApplicationServices.Commands;
using OKEService.Utilities;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.UpdateDeliveryPriceRoyal
{
    public class UpdateDeliveryPriceRoyalCommandHandler : CommandHandler<UpdateDeliveryPriceRoyalCommand>
    {
        private readonly IOrderQueryRepository query;
        private readonly IOrderCommandRepository command;

        public UpdateDeliveryPriceRoyalCommandHandler(OKEServiceServices okeserviceServices, IOrderQueryRepository query, 
                                                      IOrderCommandRepository command) : base(okeserviceServices)
        {
            this.query = query;
            this.command = command;
        }

        public override async Task<CommandResult> Handle(UpdateDeliveryPriceRoyalCommand request)
        {
            var order = await this.query.FindOrderByVendor(request.OrderCode, "V00051");
            if (order == null)
                throw new Exception("Not Found Order Code");

            //order.CalDeliveryPriceRoyal();
            //await this.command.UpdateDeliveryPrice(order);
            return Ok();
        }
    }
}
