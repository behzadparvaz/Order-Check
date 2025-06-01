using OKEService.Core.ApplicationServices.Commands;
using OKEService.Utilities;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.ChangePickStatus
{
    public class ChangePickStatusCommandHandler : CommandHandler<ChangePickStatusCommand>
    {
        private readonly IOrderCommandRepository commandRepository;
        private readonly IOrderQueryRepository queryRepository;

        public ChangePickStatusCommandHandler(OKEServiceServices okeserviceServices, IOrderCommandRepository commandRepository, IOrderQueryRepository queryRepository) : base(okeserviceServices)
        {
            this.commandRepository = commandRepository;
            this.queryRepository = queryRepository;
        }

        public async override Task<CommandResult> Handle(ChangePickStatusCommand command)
        {
            //var order = queryRepository.GetOrderByOrderCode(command.OrderCode);            
            //if (order == null)
            //    throw new Exception("Order not found!");
            //if (order.OrderStatus.Id != OrderStatus.Paid.Id)
            //    throw new Exception("Invalid Status.");

            //var orderStatus = OrderStatus.From(command.StatusId);            
            //await commandRepository.UpdateStatus(command.OrderCode, command.PhoneNumber, orderStatus);
            return Ok();
        }
    }
}
