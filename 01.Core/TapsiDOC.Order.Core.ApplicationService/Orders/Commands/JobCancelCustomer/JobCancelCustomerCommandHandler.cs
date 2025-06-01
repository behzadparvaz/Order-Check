using MediatR;
using Microsoft.Extensions.Logging;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.JobCancelCustomer
{
    public class JobCancelCustomerCommandHandler : IRequestHandler<JobCancelCustomerCommand>
    {
        private readonly IOrderCommandRepository command;
        private readonly IOrderQueryRepository query;
        private readonly ILogger<JobCancelCustomerCommandHandler> logger;

        public JobCancelCustomerCommandHandler(IOrderCommandRepository command,
            IOrderQueryRepository query,
            ILogger<JobCancelCustomerCommandHandler> logger)
        {
            this.command = command;
            this.query = query;
            this.logger = logger;
        }

        public async Task<Unit> Handle(JobCancelCustomerCommand request, CancellationToken cancellationToken)
        {
            var nullOrders = await this.query.FindCurrentDayOrders();
            if (nullOrders.Count == 0)
                return Unit.Value;
            var cancelOrders = await this.query.FindCancelOrdersLog(nullOrders);
            foreach (var item in cancelOrders)
            {
                await this.command.UpdateCancelCustomerV2(item.OrderCode, item.StatusId, item.StatusName,
                                            item.DeclineId, item.DeclineName);
            }
            return Unit.Value;
        }
    }
}
