using MediatR;
using Microsoft.Extensions.Logging;
using TapsiDOC.Order.Core.ApplicationService.Orders.Commands.OrderScheduled;
using TapsiDOC.Order.Core.Domain.Contracts;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.OrderScheduledFoAllVendors
{
    public class OrderScheduledFoAllVendorsCommandHandler : IRequestHandler<OrderScheduledFoAllVendorsCommand>
    {
        private readonly IOrderCommandRepository command;
        private readonly IRequestOrder requestOrder;
        private readonly IOrderQueryRepository query;
        private readonly ILogger<OrderScheduledCommandHandler> logger;
        private readonly IOrderSmsSender smsSender; 

        public OrderScheduledFoAllVendorsCommandHandler(IOrderCommandRepository command,
            IRequestOrder requestOrder,
            IOrderQueryRepository query,
            IOrderSmsSender smsSender,
            ILogger<OrderScheduledCommandHandler> logger)
        {
            this.smsSender = smsSender; 
            this.command = command;
            this.requestOrder = requestOrder;
            this.query = query;
            this.logger = logger;
        }

        public async Task<Unit> Handle(OrderScheduledFoAllVendorsCommand request, CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Start OrderScheduledFoAllVendors");

            var outBoxes = await this.query.OutBoxEventItemsInLastThirtyMinutes();

            //this.logger.LogInformation($"Count Order: {outBoxes.Count()}");

            var ackOrders = await this.query.GetAckOrders(outBoxes.Select(x => x.OrderCode).ToList());

            //this.logger.LogInformation("Ack Orders are: {@ackOrders}", ackOrders);

            outBoxes = outBoxes.Where(x => ackOrders.Any(a => a.OrderCode == x.OrderCode)).ToList();

            //this.logger.LogInformation("Final Orders are : {@outBoxes}", outBoxes);

            foreach (var outbox in outBoxes)
            {
                var order = ackOrders.Where(x => x.OrderCode == outbox.OrderCode).FirstOrDefault();

                this.logger.LogInformation("Order is: {@order}", order);

                bool NeedAllCityVendors = true;

                var tender = await this.query.TenderVendors(order.Customer.Addresses.First().Latitude,
                                                            order.Customer.Addresses.First().Longitude,
                                                            NeedAllCityVendors);

                this.logger.LogInformation("Tenders In OrderScheduledFoAllVendors: {@tender}", tender);

                if (tender == null || tender.Count == 0)
                    break;  

                else
                {
                    double minLat = 35.5;
                    double maxLat = 35.9;
                    double minLng = 51.12;
                    double maxLng = 51.6;

                    double orderLat = order.Customer.Addresses.FirstOrDefault().Latitude;
                    double orderLng = order.Customer.Addresses.FirstOrDefault().Longitude;
                    bool isInTehran = (orderLat >= minLat && orderLat <= maxLat) && (orderLng >= minLng && orderLng <= maxLng);

                    if (tender.Count() > 0 && isInTehran)
                    {
                        foreach (var vendorTender in tender)
                        {
                            if (!string.IsNullOrEmpty(vendorTender))
                            {
                                order.Delivery.IsScheduled = true;
                                order.VendorCode = vendorTender;
                                var check = await this.query.FindOrderByVendor(order.OrderCode, vendorTender);

                                this.logger.LogInformation("Order in last step is: {@order}", order);

                                if (check == null)
                                    await this.command.OrderScheduled(order, outbox.RecId, outbox.ParentTraceId);
                            }
                        }
                    }
                    await smsSender.SendNotificationScheduledOrder(order);
                }

            }
            return Unit.Value;
        }
    }
}
