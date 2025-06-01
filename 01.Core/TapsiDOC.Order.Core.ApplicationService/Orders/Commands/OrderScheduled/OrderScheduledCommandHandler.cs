using MediatR;
using Microsoft.Extensions.Logging;
using TapsiDOC.Order.Core.Domain.Contracts;
using TapsiDOC.Order.Core.Domain.Orders.Entities;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.OrderScheduled
{
    public class OrderScheduledCommandHandler : IRequestHandler<OrderScheduledCommand>
    {
        private readonly IOrderCommandRepository command;
        private readonly IRequestOrder requestOrder;
        private readonly IOrderQueryRepository query;
        private readonly ILogger<OrderScheduledCommandHandler> logger;

        public OrderScheduledCommandHandler(IOrderCommandRepository command,
            IRequestOrder requestOrder,
            IOrderQueryRepository query,
            ILogger<OrderScheduledCommandHandler> logger)
        {
            this.command = command;
            this.requestOrder = requestOrder;
            this.query = query;
            this.logger = logger;
        }

        public async Task<Unit> Handle(OrderScheduledCommand request, CancellationToken cancellationToken)
        {
           
                this.logger.LogInformation("Start Scheduled");
                var outBoxs = await this.query.OutBoxEventItems();
                this.logger.LogInformation($"Count Order:{outBoxs.Count()}");
                foreach (var outbox in outBoxs)
                {
                    if (outbox.OrderCode.Length >= 10)
                        return Unit.Value;
                    var order = await this.query.GetOrderByOrderCode(outbox.OrderCode);
                    if (order.OrderStatus.Id >= OrderStatus.Pick.Id)
                        await this.command.UpdateOutBoxEvent(outbox.RecId);
                    else
                    {
                        if (order.VendorCode == "V00001")
                            await this.command.OrderScheduled(order, outbox.RecId, outbox.ParentTraceId);
                        else
                        {
                            double minLat = 35.5;
                            double maxLat = 35.9;
                            double minLng = 51.2;
                            double maxLng = 51.6;
                            var tender = await this.query.TenderVendors(order.Customer.Addresses.First().Latitude,
                                order.Customer.Addresses.First().Longitude, false);
                            if (tender == null)
                                await this.command.UpdateOutBoxEvent(outbox.RecId);
                            if (tender?.Count == 0)
                                await this.command.UpdateOutBoxEvent(outbox.RecId);

                            var royal = order.OrderDetails.Find(a => a.Type.Id == OrderDetailType.Supplement.Id);
                            double orderLat = order.Customer.Addresses.FirstOrDefault().Latitude;
                            double orderLng = order.Customer.Addresses.FirstOrDefault().Longitude;
                            bool isInTehran = (orderLat >= minLat && orderLat <= maxLat) && (orderLng >= minLng && orderLng <= maxLng);
                            if (isInTehran)
                            {
                                if (royal != null)
                                {
                                    tender.Add("V000164");
                                    tender.Add("V000170");
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

                            //var requestOrder = order.OrderDetails.FindAll(a => a.Type.Id == OrderDetailType.RequestOrder.Id);
                            //if (requestOrder.Count > 0)
                            //{
                            //    List<OrderDetail> orderDetails = new List<OrderDetail>();
                            //    foreach (var item in requestOrder)
                            //        orderDetails.Add(OrderDetail.Create(string.Empty,
                            //                                                string.Empty,
                            //                                                item.ProductName,
                            //                                                0,
                            //                                                0,
                            //                                                0,
                            //                                                item.Quantity,
                            //                                                "",
                            //                                                OrderDetailType.RequestOrder.Id,
                            //                                                item.Unit,
                            //                                                item.Description));

                            //    await this.requestOrder.SendRequestOrder(orderDetails,
                            //                                                order.OrderCode,
                            //                                                order.Customer.NationalCode,
                            //                                                order.Customer.AlternateRecipientMobileNumber != null ? order.Customer.AlternateRecipientMobileNumber : order.Customer.PhoneNumber,
                            //                                                order.Customer.Name,
                            //                                                order.Comment,
                            //                                                outbox.Token);
                            //}
                        }
                    }
                }
                return Unit.Value;
            //}
            //catch (Exception ex)
            //{
            //    this.logger.LogInformation($"Error:{ex.Message}");
            //    throw new Exception(ex.Message);
            //}
        }
    }
}
