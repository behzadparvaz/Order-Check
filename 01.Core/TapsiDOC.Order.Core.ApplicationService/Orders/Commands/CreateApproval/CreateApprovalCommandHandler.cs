using OKEService.Core.ApplicationServices.Commands;
using OKEService.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TapsiDOC.Order.Core.ApplicationService.Orders.Commands.DoPayment;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.CreateApproval
{
    public class CreateApprovalCommandHandler : CommandHandler<CreateApprovalCommand>
    {
        private readonly IOrderCommandRepository commandRepository;
        private readonly IOrderQueryRepository queryRepository;

        public CreateApprovalCommandHandler(OKEServiceServices okeserviceServices, IOrderCommandRepository commandRepository, IOrderQueryRepository queryRepository) : base(okeserviceServices)
        {
            this.commandRepository = commandRepository;
            this.queryRepository = queryRepository;
        }

        public async override Task<CommandResult> Handle(CreateApprovalCommand command)
        {
            //var order = queryRepository.GetOrderByOrderCode(command.OrderCode);
            //if (order == null)
            //    throw new Exception("Order not found!");
            //order.SetApprovalStatus();
            //await commandRepository.RaiseEvents(order);            
            return Ok();
        }
    }
}
