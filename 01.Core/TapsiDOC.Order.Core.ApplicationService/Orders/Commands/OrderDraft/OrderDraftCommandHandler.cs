using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OKEService.Utilities;
using TapsiDOC.Order.Core.Domain.Contracts;
using TapsiDOC.Order.Core.Domain.Orders.Entities;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;
using Aggreagte = TapsiDOC.Order.Core.Domain.Orders.Entities;
using MyEntities = TapsiDOC.Order.Core.Domain.Orders.Entities;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.OrderDraft
{
    public class OrderDraftCommandHandler : IRequestHandler<OrderDraftCommand, string>
    {
        private readonly IOrderCommandRepository command;
        private readonly IOrderQueryRepository query;
        private readonly IOrderSmsSender smsSender;
        private readonly IRequestOrder requestOrder;
        private readonly ILogger<OrderDraftCommandHandler> logger;

        public OrderDraftCommandHandler(OKEServiceServices okeserviceServices,
            IOrderCommandRepository command,
            IOrderSmsSender smsSender,
            IRequestOrder requestOrder,
            IOrderQueryRepository query,
            ILogger<OrderDraftCommandHandler> logger)
        {
            this.command = command;
            this.query = query;
            this.requestOrder = requestOrder;
            this.smsSender = smsSender;
            this.logger = logger;
        }

        public async Task<string> Handle(OrderDraftCommand request, CancellationToken cancellationToken)
        {
            request.orderCode = MyEntities.Order.RandomString(6);
            // Aggreagte.Order? order= null;
            // if (request.IsSpecialPatient && !string.IsNullOrEmpty(request.VendorCode))
            // {
            //     order = await CreateOrder(request);
            // }
            // else
            //{    
            //     //var tender = await this.query.TenderVendors(request.Latitude, request.Longitude);

            //     //if (tender.Count() > 0)
            //     //{
            //     //    foreach (var vendorTender in tender)
            //     //    {
            //     //        request.VendorCode = vendorTender;
            //     //        order = await CreateOrder(request);
            //     //    }
            //     //}

            //     //var requestOrder = request.Items.FindAll(a => a.ProductType == OrderDetailType.RequestOrder.Id);
            //     //if (requestOrder.Count > 0)
            //     //{
            //     //    List<OrderDetail> orderDetails = new List<OrderDetail>();
            //     //    foreach (var item in requestOrder)
            //     //        orderDetails.Add(OrderDetail.Create(string.Empty,
            //     //                                                string.Empty,
            //     //                                                item.ProductName,
            //     //                                                0,
            //     //                                                0,
            //     //                                                0,
            //     //                                                item.Quantity,
            //     //                                                "",
            //     //                                                OrderDetailType.RequestOrder.Id,
            //     //                                                item.Unit,
            //     //                                                item.Description));

            //     //    await this.requestOrder.SendRequestOrder(orderDetails,
            //     //                                                request.orderCode,
            //     //                                                request.NationalCode,
            //     //                                                request.Comment,
            //     //                                                request.Token);
            //     //}
            //     order = await CreateOrder(request);
            // }

            // await smsSender.SendNotification(order);
            logger.LogDebug($"Start OrderDraftCommandHandler");

            var order = await CreateOrder(request);
            _= smsSender.SendNotification(order);

            logger.LogDebug("OrderDraftCommandHandler Done");

            return order.OrderCode;
        }


        private async Task<Core.Domain.Orders.Entities.Order> CreateOrder(OrderDraftCommand request)
        {
            logger.LogDebug($"Start CreateOrder");
            logger.LogDebug("CreateOrder Input: {@request}", request);
            
            List<OrderDetail> orderDetails = new List<OrderDetail>();
            if (request.Items.Count > 0)
            {
                foreach (var item in request.Items)
                {
                    if (!string.IsNullOrEmpty(item.ReferenceNumber))
                        orderDetails.Add(OrderDetail.CreateReferenceNumber(item.ReferenceNumber));
                    else
                        orderDetails.Add(OrderDetail.Create(item.IRC, item.GTIN, item.ProductName, 0, 0, 0,
                                                            item.Quantity, item.ImageLink,
                                                            item.ProductType, item.Unit,
                                                            item.Description,item.AttachmentId));
                }
            }
            
            var order = Aggreagte.Order.CreateOrder(request.orderCode, request.InsuranceTypeId,
                                        request.SupplementaryInsuranceTypeId, request.FromDeliveryTime,
                                        request.ToDeliveryTime, request.DeliveryDate, request.IsSpecialPatient,
                                        PaymentType.Online, request.PhoneNumber,
                                        request.NationalCode, request.Latitude, request.Longitude,
                                        request.ValueAddress, request.TitleAddress,
                                        request.HouseNumber, 0, 0, request.CustomerName,
                                        request.Comment, request.HomeUnit,
                                        request.VendorCode,
                                        request.AlternateRecipientName,
                                        request.AlternateRecipientMobileNumber,
                                        orderDetails: orderDetails);

            await this.command.OrderDraft(order, request.Token);
            logger.LogDebug("OrderDraft Output: {@order}", order);
            return order;
        }
    }
}