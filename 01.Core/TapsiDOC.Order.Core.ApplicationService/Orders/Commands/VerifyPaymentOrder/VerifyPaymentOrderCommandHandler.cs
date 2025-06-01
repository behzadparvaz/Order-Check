using Microsoft.Extensions.Logging;
using OKEService.Core.ApplicationServices.Commands;
using OKEService.Core.ApplicationServices.Common;
using OKEService.Core.Domain.Data;
using OKEService.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TapsiDOC.Order.Core.Domain.Contracts;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.VerifyPaymentOrder
{
    public class VerifyPaymentOrderCommandHandler : CommandHandler<VerifyPaymentOrderCommand, PaymentData>
    {
        private readonly IOrderCommandRepository command;
        private readonly IOrderQueryRepository query;
        private readonly ILogger<VerifyPaymentOrderCommandHandler> logger;

        public VerifyPaymentOrderCommandHandler(OKEServiceServices okesrviceServices ,
            IOrderCommandRepository command,
            IOrderQueryRepository query,
            ILogger<VerifyPaymentOrderCommandHandler> logger) : base(okesrviceServices)
        {
            this.command = command;
            this.query = query;
            this.logger = logger;
        }

        public override async Task<CommandResult<PaymentData>> Handle(VerifyPaymentOrderCommand request)
        {
            logger.LogDebug("VerifyPaymentOrderCommandHandler started");
            logger.LogDebug("VerifyPaymentOrderCommandHandler request: {@request}", request);

            var result = await this.command.VerifyPayment(request.TrackId, request.OrderCode);
            if (!string.IsNullOrEmpty(result.VendorCode) && result.IsSuccess)
            {
                var order = await query.FindOrderByVendor(request.OrderCode, result.VendorCode);
                if (order == null)
                    throw new Exception("Order not found!");
                
                order.SetPickStatus();
                order.SetPickState();
                order.SetPreprationTime();

                await command.UpdatePickOrder(order);
                //await this.command.SaveEventsAsync(order.Id, order, order.OrderStatus.Id);

                logger.LogDebug("VerifyPaymentOrderCommandHandler Done");

                return Result(result , ApplicationServiceStatus.Ok);
            }
            return Result(result, ApplicationServiceStatus.Ok);
        }
    }
}
