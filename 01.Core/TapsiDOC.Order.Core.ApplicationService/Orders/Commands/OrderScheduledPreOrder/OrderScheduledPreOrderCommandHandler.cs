using MediatR;
using Microsoft.Extensions.Logging;
using TapsiDOC.Order.Core.ApplicationService.Orders.Commands.OrderScheduled;
using TapsiDOC.Order.Core.Domain.Contracts;
using TapsiDOC.Order.Core.Domain.Orders.Entities;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;
using TapsiDOC.Order.Core.Domain.Orders.Entities;
using System.Collections.Generic;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.OrderScheduledPreOrder
{
    public class OrderScheduledPreOrderCommandHandler : IRequestHandler<OrderScheduledPreOrderCommand>
    {
        private readonly IOrderCommandRepository command;
        private readonly IRequestOrder requestOrder;
        private readonly IOrderQueryRepository query;
        private readonly ILogger<OrderScheduledPreOrderCommandHandler> logger;

        public OrderScheduledPreOrderCommandHandler(IOrderCommandRepository command,
            IRequestOrder requestOrder,
            IOrderQueryRepository query,
            ILogger<OrderScheduledPreOrderCommandHandler> logger)
        {
            this.command = command;
            this.requestOrder = requestOrder;
            this.query = query;
            this.logger = logger;
        }

        public async Task<Unit> Handle(OrderScheduledPreOrderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                this.logger.LogInformation("Start OrderScheduledPreOrderCommand");

                var outBox = await this.query.OutBoxEventItemsOverNight();

                this.logger.LogInformation($"Count Order in OrderScheduledPreOrderCommand:{outBox.Count()}");

                var ordersInPickStatus = (await this.query.GetOrdersByOrderCodes
                                         (outBox.Select(x => x.OrderCode).ToList()))
                                         .Where(x => x.OrderStatus.Id == OrderStatus.Pick.Id || x.OrderStatus.Id == OrderStatus.CancelCustomer.Id)
                                         .Select(x => x.OrderCode).ToList();

                outBox = outBox.Where(order => !ordersInPickStatus.Contains(order.OrderCode)).ToList();

                foreach (var outbox in outBox)
                {
                    if (outbox.OrderCode.Length >= 10)
                        return Unit.Value;

                    var order = await this.query.GetOrderByOrderCode(outbox.OrderCode);

                    double minLat = 35.5;
                    double maxLat = 35.9;
                    double minLng = 51.2;
                    double maxLng = 51.6;
                    var tender = await this.query.TenderVendorsOverNight(order.Customer.Addresses.First().Latitude, order.Customer.Addresses.First().Longitude,
                                                                "A++", 7);
                    if (tender == null)
                        await this.command.UpdateOutBoxEvent(outbox.RecId);
                    if (tender.Count == 0)
                        await this.command.UpdateOutBoxEvent(outbox.RecId);

                    var royal = order.OrderDetails.Find(a => a.Type.Id == OrderDetailType.Supplement.Id);
                    double orderLat = order.Customer.Addresses.FirstOrDefault().Latitude;
                    double orderLng = order.Customer.Addresses.FirstOrDefault().Longitude;
                    bool isInTehran = (orderLat >= minLat && orderLat <= maxLat) && (orderLng >= minLng && orderLng <= maxLng)
                        ;
                    if (isInTehran)
                    {
                        if (royal != null)
                        {
                            tender.Add("V00051");
                        }
                    }
                    if (tender.Count() > 0)
                    {
                        foreach (var vendorTender in tender)
                        {
                            if (!string.IsNullOrEmpty(vendorTender))
                            {
                                order.VendorCode = vendorTender;
                                var check = await this.query.FindOrderByVendor(order.OrderCode, vendorTender);
                                if (check == null)
                                    await this.command.OrderScheduled(order, outbox.RecId, outbox.ParentTraceId);
                            }
                        }
                    }
                }

                return Unit.Value;
            }
            catch (Exception ex)
            {
                this.logger.LogInformation($"Error:{ex.Message}");
                throw new Exception(ex.Message);
            }
        }
    }
}
