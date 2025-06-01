using Microsoft.Extensions.Logging;
using OKEService.Core.ApplicationServices.Commands;
using OKEService.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TapsiDOC.Order.Core.ApplicationService.Orders.Commands.VerifyPaymentOrder;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.InquiryPayment
{
    public class InquiryPaymentCommandHandler : CommandHandler<InquiryPaymentCommand , bool>
    {
        private readonly IOrderCommandRepository command;
        private readonly IOrderQueryRepository query;
        private readonly ILogger<VerifyPaymentOrderCommandHandler> logger;
        public InquiryPaymentCommandHandler(OKEServiceServices okeserviceServices ,
               IOrderCommandRepository command,
            IOrderQueryRepository query,
            ILogger<VerifyPaymentOrderCommandHandler> logger) : base(okeserviceServices)
        {
            this.command = command;
            this.query = query;
            this.logger = logger;
        }

        public async override Task<CommandResult<bool>> Handle(InquiryPaymentCommand request)
        {
            var result = await this.command.VerifyPayment(request.TrackId, request.OrderCode);
            if (!string.IsNullOrEmpty(result.VendorCode))
            {
                var order = await query.FindOrderByVendor(request.OrderCode, result.VendorCode);
                if (order == null)
                    throw new Exception("Order not found!");
                logger.LogDebug("VerifyPaymentOrderCommandHandler Done");
                return Ok(true);
            }
            throw new ArgumentException("شماره تراکنش وارد شد صحیح نیست");
        }
    }
}
