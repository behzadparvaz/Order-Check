using OKEService.Core.Domain.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using TapsiDOC.Order.Core.Domain.Contracts;
using TapsiDOC.Order.Core.Domain.Orders.CommonContract;
using TapsiDOC.Order.Core.Domain.Orders.Entities;
using TapsiDOC.Order.Core.Domain.Orders.QueryContract;
using Aggregate = TapsiDOC.Order.Core.Domain.Orders.Entities;

namespace TapsiDOC.Order.Core.Domain.Orders.Repositories
{
    public interface IOrderQueryRepository
    {
        Task<Vendor> GetVendorByVendorCode(string vendorCode);
        Task<Aggregate.Order> GetOrderByOrderCode(string orderCode);
        Task<List<Aggregate.Order>> GetOrdersHistory(string phoneNumber, int statusId);
        Task<Aggregate.Order> FindOrder(string orderCode, string phoneNumber);
        Task<Aggregate.Order> FindOrderByVendor(string orderCode, string vendorCode);
        Task<Aggregate.Order> FindLastOrderByUser(string phoneNumber);
        Task<List<Aggregate.Order>> FindOrderByPhoneNumber(string phoneNumber);
        Task<List<VendorType>> GetVendors();
        Task<PagedData<Aggregate.Order>> Tenders(string orderCode);
        Task<List<string>> TenderVendors(double latitude, double longitude, bool needAllCityVendors);
        Task<Aggregate.Order> GetOrderDraft(string phoneNumber, string orderCode);
        Task<List<Aggregate.Order>> GetOrder(string orderCode);
        Task<decimal> GetDeliveryPrice(Aggregate.Order order);
        Task<Aggregate.Order> GetOrderDraftByUser(string phoneNumber);
        Task<long> GetDailyOrdersCountByPhoneNumber(string phoneNumber);
        Task<List<Aggregate.Order>> GetOrderAPay(string orderCode);
        Task<List<OutBoxEvent>> OutBoxEventItems();
        Task<List<Aggregate.Order>> FetchAuctionOrder();
        Task<string> FindPhoneNumber(string orderCode);
        Task<Aggregate.Order> FindOrderByVendorReyall(string orderCode, string vendorCode);
        Task<List<Aggregate.Order>> GetOrderForCoupon(int[] statuses, string orderCode);
        Task<List<OutBoxEvent>> OutBoxEventItemsOverNight();
        Task<List<Aggregate.Order>> GetOrdersByOrderCodes(List<string> orderCodes);
        Task<List<string>> TenderVendorsOverNight(double latitude, double longitude, string grade, double radius);
        Task<List<string>> FindCurrentDayOrders();
        Task<List<CancelOrder>> FindCancelOrdersLog(List<string> orderCodes);
        Task<List<OutBoxEvent>> OutBoxEventItemsInLastThirtyMinutes();
        Task<List<Aggregate.Order>> GetAckOrders(List<string> orderCode);
       // Task<List<VendorType>> GetAllVendors(double lat, double lng);
    }
}