using Microsoft.Extensions.Logging;
using OKEService.Core.ApplicationServices.Commands;
using OKEService.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;
using MyEntities = TapsiDOC.Order.Core.Domain.Orders.Entities;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.AlternativeProduct
{
    public class AlternativeProductCommandHandler : CommandHandler<AlternativeProductCommand, bool>
    {
        private readonly IOrderCommandRepository command;
        private readonly IOrderQueryRepository query;
        private readonly ILogger<AlternativeProductCommandHandler> logger;

        public AlternativeProductCommandHandler(OKEServiceServices okesrviceServices,
            OKEServiceServices okeserviceServices,
            IOrderCommandRepository command,
            IOrderQueryRepository query,
            ILogger<AlternativeProductCommandHandler> logger) : base(okesrviceServices)
        {
            this.command = command;
            this.query = query;
            this.logger = logger;
        }

        public async override Task<CommandResult<bool>> Handle(AlternativeProductCommand request)
        {
            logger.LogDebug("AlternativeProductCommandHandler started");
            logger.LogDebug("AlternativeProductCommandHandler request: {@request}", request);

            if (request.AggregateType == "OMS")
            {
                if (!request.EventType.Equals("nfc"))
                    throw new ArgumentException("خطا در اطلاع رسانی ایونت اشتباه است");
                var result = JsonSerializer.Deserialize<MyEntities.Order>(request.Event);
                var order = await query.FindOrderByVendor(result.OrderCode, result.VendorCode);
                if (order == null)
                    throw new ArgumentException("شماره سفارش وجود ندارد");
                order.SetNFCStatus();
                order.SetDescription(result.Description.Comment, result.Description.Link, result.TotalPrice,
                                    result.PackingPrice, result.SupervisorPharmacyPrice, 0);
                var res = await command.DescriptionProduct(order);

                logger.LogDebug("AlternativeProductCommandHandler Done");

                return Ok(res);
            }
            else
            {
                logger.LogDebug("AlternativeProductCommandHandler Done: Aggregate Type is OMS.");

                throw new ArgumentException("خطا در دریافت اطلاع رسانی کنسل توسط صاحب نسخه");
            }
        }
    }
}
