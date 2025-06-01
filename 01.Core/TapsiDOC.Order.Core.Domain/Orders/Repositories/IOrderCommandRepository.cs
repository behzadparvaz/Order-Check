using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TapsiDOC.Order.Core.Domain.Contracts;
using TapsiDOC.Order.Core.Domain.Orders.Entities;

namespace TapsiDOC.Order.Core.Domain.Orders.Repositories
{
    public interface IOrderCommandRepository
    {
        Task CreateOrder(Order.Core.Domain.Orders.Entities.Order order);
        Task OrderDraft(Order.Core.Domain.Orders.Entities.Order order , string token);
        Task OrderScheduled(Order.Core.Domain.Orders.Entities.Order order , long updateOutBox, string parentSpanId);
        Task AssignmentOrder(Order.Core.Domain.Orders.Entities.Order order);
        Task UpdateStatus(Entities.Order order);
        Task UpdateCancelCustomer(Entities.Order order);
        Task UpdateCancelCustomerV2(string orderCode , int statusId , string statusName , int declineId , string declineName);
        Task UpdatePickOrder(Entities.Order order);
        Task<EventData> SaveEventsAsync(string aggregateId, Order.Core.Domain.Orders.Entities.Order order , int expectedVersion);
        Task<string> Payment(string orderCode, string phoneNumber, decimal amount , string vendorCode);
        Task DeliveryStatus(string orderCode , string statusName);
        Task<PaymentData> VerifyPayment(string trackId, string orderCode);
        Task<bool> DescriptionProduct(Order.Core.Domain.Orders.Entities.Order order);
        Task<List<string>> CreateTender(string orderCode , double latitude , double longitude);
        Task UpdateOrderDetails(Order.Core.Domain.Orders.Entities.Order order);
        Task UpdateOrderReject(Order.Core.Domain.Orders.Entities.Order order);
        Task UpdateOutBoxEvent(long updateId);
        Task RaiseEventDeleteBasket(Order.Core.Domain.Orders.Entities.Order order, string token);
        Task RaiseEventDeleteBasketScheduler(Order.Core.Domain.Orders.Entities.Order order, string token, long? recId);
        Task ChangeStateOrderApay(Order.Core.Domain.Orders.Entities.Order order);
        Task UpdateDeliveryRoyal(Order.Core.Domain.Orders.Entities.Order order);
        Task<bool> UpdateDelivery(Entities.Order order);
        Task<bool> AddReview(Review review);
    }
}
