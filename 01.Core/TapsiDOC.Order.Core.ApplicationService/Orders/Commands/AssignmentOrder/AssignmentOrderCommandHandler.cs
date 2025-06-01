using OKEService.Core.ApplicationServices.Commands;
using OKEService.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.AssignmentOrder
{
    public class AssignmentOrderCommandHandler : CommandHandler<AssignmentOrderCommand>
    {
        private readonly IOrderCommandRepository command;
        private readonly IOrderQueryRepository query;

        public AssignmentOrderCommandHandler(OKEServiceServices okeserviceServices, IOrderCommandRepository command,
                                        IOrderQueryRepository query) : base(okeserviceServices)
        {
            this.command = command;
            this.query = query;
        }

        public override async Task<CommandResult> Handle(AssignmentOrderCommand request)
        {
            var order = await this.query.GetOrderByOrderCode(request.OrderCode);
            order.VendorCode = request.VendorCode;
            order.SetAckState();
            await this.command.AssignmentOrder(order);
            return Ok();

        }
    }
}
