using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OKEService.Core.Domain.Data;
using Polly;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Retry;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading;
using TapsiDOC.Order.Core.Domain.Contracts;
using TapsiDOC.Order.Core.Domain.Orders.CommonContract;
using TapsiDOC.Order.Core.Domain.Orders.Entities;
using TapsiDOC.Order.Core.Domain.Orders.QueryContract;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;
using TapsiDOC.Order.Infra.Data.Sql.Queries.Orders.Context;
using TapsiDOC.Order.Infra.Data.Sql.Queries.Orders.ContextSetting;
using TapsiDOC.Order.Infra.Data.Sql.Queries.Orders.DTOs;
using Aggregate = TapsiDOC.Order.Core.Domain.Orders.Entities;

namespace TapsiDOC.Order.Infra.Data.Sql.Queries.Orders
{

    public class OrderQueryRepository : IOrderQueryRepository
    {
        private readonly OrderContext _context = null;
        private readonly IOptions<Settings> _settings;
        private readonly IConfiguration _config;
        private readonly ILogger<OrderQueryRepository> _logger;
        private string ConnectionString = "SqlConnectionString";
        private string _fetchImageProducts = "FetchImageProducts";
        private string _priceInquiryOnDemand = "PriceInquiryOnDemand";
        private string LoggDataCnn = "loggDataConnectionString";
        private const string _sqlOutBox = "SqlOutBox";
        private string _findVendor = "FindVendorLocation";
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
        private readonly AsyncFallbackPolicy<HttpResponseMessage> _fallbackPolicy;

        private static Polly.CircuitBreaker.AsyncCircuitBreakerPolicy<HttpResponseMessage> _circuitBreaker =
            Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode).Or<HttpRequestException>()
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(60),
                    (d, c) =>
                    {
                        string a = "Break";
                    },
                    () =>
                    {
                        string a = "Reset";
                    },
                    () =>
                    {
                        string a = "Half";
                    });

        public OrderQueryRepository(IOptions<Settings> settings,
            IConfiguration config,
            ILogger<OrderQueryRepository> logger)
        {
            _context = new OrderContext(settings);
            _settings = settings;
            _config = config;
            _logger = logger;
            _retryPolicy = Policy
                .HandleResult<HttpResponseMessage>(result => !result.IsSuccessStatusCode)
                .RetryAsync(1, (d, c) =>
                {
                    string a = "Retry";
                });

            _fallbackPolicy = Policy.HandleResult<HttpResponseMessage>(result => (int)result.StatusCode == 500)
                .Or<BrokenCircuitException>()
                .FallbackAsync(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
                {
                    Content = new ObjectContent(typeof(MessageDto), new MessageDto
                    {
                        Id = 100,
                        Text = "Retry Send Data"
                    }, new JsonMediaTypeFormatter())
                });
        }


        public async Task<List<Aggregate.Order>> GetOrdersHistory(string phoneNumber, int statusId)
        {
            try
            {
                //BsonClassMap.RegisterClassMap<Aggregate.Order>(cm => { cm.AutoMap(); cm.SetIgnoreExtraElements(true); });
                int[] status = null;
                FilterDefinition<Aggregate.Order> filter = null;

                if (statusId == -1)
                {
                    status = [OrderStatus.Draft.Id, OrderStatus.Ack.Id,OrderStatus.APay.Id, OrderStatus.Pick.Id,
                                OrderStatus.NFC.Id, OrderStatus.Accept.Id, OrderStatus.ADelivery.Id,
                                OrderStatus.SendDelivery.Id, OrderStatus.Deliverd.Id,
                                OrderStatus.Return.Id]; //[0, 1, 2, 3, 4, 5, 6, 7, 8, 9];
                    filter = Builders<Aggregate.Order>.Filter.And(
                        Builders<Aggregate.Order>.Filter.Eq("Customer.PhoneNumber", phoneNumber),
                        Builders<Aggregate.Order>.Filter.In("OrderStatus._id", status));
                }
                else
                {
                    switch (statusId)
                    {
                        case 0:
                            status = [OrderStatus.Draft.Id, OrderStatus.Ack.Id ,OrderStatus.APay.Id , OrderStatus.Pick.Id,
                                OrderStatus.NFC.Id, OrderStatus.Accept.Id, OrderStatus.ADelivery.Id,
                                OrderStatus.SendDelivery.Id, OrderStatus.Deliverd.Id,
                                OrderStatus.Return.Id];
                            break;

                        case 1:
                            status = [OrderStatus.Deliverd.Id];
                            break;

                        case 2:
                            status = [OrderStatus.CancelCustomer.Id, OrderStatus.CancelVendor.Id, OrderStatus.Reject.Id];
                            break;
                    }

                    filter = Builders<Aggregate.Order>.Filter.And(
                        Builders<Aggregate.Order>.Filter.Eq("Customer.PhoneNumber", phoneNumber),
                        Builders<Aggregate.Order>.Filter.In("OrderStatus._id", status));
                }

                var projection = Builders<Aggregate.Order>.Projection
                              .Exclude("Customer");

                var result = await _context.OrdersCollection.Find(filter)
                     .Project<Aggregate.Order>(projection)
                     .Sort("{CreateDateTime:-1 , \"OrderStatus._id\":-1}").ToListAsync();

                //foreach (var item in result)
                //{
                //    var products = item.OrderDetails.Select(a => a.IRC);
                //    var imageData = await fetchImageProduct(products.ToList());
                //    if (imageData != null)
                //    {
                //        foreach (var item1 in imageData)
                //        {
                //            item.OrderDetails.Find(a => a.IRC == item1.IRC).ImageLink = item1.ImageLink;
                //        }
                //    }
                //}

                return result.GroupBy(order => order.OrderCode)
                             .Select(group => group.OrderByDescending(o => o.OrderStatus.Id).First())
                             .OrderByDescending(z => DateTime.Parse(z.CreateDateTime))
                             .ToList();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError($"GetOrdersHistory() Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<Aggregate.Order> GetOrderByOrderCode(string orderCode)
        {
            try
            {
                var filter = Builders<Aggregate.Order>.Filter.Eq("OrderCode", orderCode);
                var projection = Builders<Aggregate.Order>.Projection.Exclude(u => u.Id);
                var result = await _context.OrdersCollection.FindAsync(filter);
                return result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetOrderByOrderCode() OrderCode {orderCode}: exception = {ex.Message}");
                throw new ArgumentException($"Exception  order code = {orderCode} with error message = {ex.Message}");
            }
        }

        public async Task<Aggregate.Order> FindOrder(string orderCode, string phoneNumber)
        {
            try
            {
                var filter = Builders<Aggregate.Order>.Filter.And(
                    Builders<Aggregate.Order>.Filter.Eq("OrderCode", orderCode),
                    Builders<Aggregate.Order>.Filter.Eq("Customer.PhoneNumber", phoneNumber)
                );
                var projection = Builders<Aggregate.Order>.Projection.Exclude(u => u.Id);
                var result = _context.OrdersCollection.Find(filter).Project<Aggregate.Order>(projection)
                    .FirstOrDefaultAsync().Result;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetOrdersHistory() Exception: {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        private string PersianDateTime(DateTime date, int time)
        {
            PersianCalendar p = new PersianCalendar();
            return string.Format(@"{0:0000}/{1:00}/{2:00} {3:00}:{4:00}",
                p.GetYear(date.AddMinutes(time)),
                p.GetMonth(date.AddMinutes(time)),
                p.GetDayOfMonth(date.AddMinutes(time)),
                p.GetHour(date.AddMinutes(time)),
                p.GetMinute(date.AddMinutes(time)));
        }
        public async Task<Aggregate.Order> FindLastOrderByUser(string phoneNumber)
        {
            try
            {
                //int[] status = [0, 1, 2, 3, 4, 5, 6, 7, 8 , 9];
                int[] status = [
                     OrderStatus.Draft.Id , OrderStatus.Ack.Id ,OrderStatus.APay.Id , OrderStatus.Pick.Id ,
                     OrderStatus.Accept.Id , OrderStatus.ADelivery.Id , OrderStatus.SendDelivery.Id ,
                     OrderStatus.Deliverd.Id
                    ];
                var today = PersianDateTime(DateTime.Now.AddDays(-1), 0);
                var tomorrow = PersianDateTime(DateTime.Now, 1440);

                var excludedOrderCodes = await _context.OrdersCollection
                                                       .Find(
                                                           Builders<Aggregate.Order>.Filter.And(
                                                               Builders<Aggregate.Order>.Filter.Eq("Customer.PhoneNumber", phoneNumber),
                                                               Builders<Aggregate.Order>.Filter.Gte("CreateDateTimeOrder", today),
                                                               Builders<Aggregate.Order>.Filter.Lt("CreateDateTimeOrder", tomorrow),
                                                               Builders<Aggregate.Order>.Filter.Eq("OrderStatus._id", OrderStatus.Deliverd.Id),
                                                               Builders<Aggregate.Order>.Filter.Eq("HasReview", true)
                                                           ))
                                                       .Project(order => order.OrderCode)
                                                       .ToListAsync();

                var filter = Builders<Aggregate.Order>.Filter.And(
                   Builders<Aggregate.Order>.Filter.Eq("Customer.PhoneNumber", phoneNumber),
                   Builders<Aggregate.Order>.Filter.In("OrderStatus._id", status),
                   Builders<Aggregate.Order>.Filter.Gte("CreateDateTimeOrder", today),
                   Builders<Aggregate.Order>.Filter.Lt("CreateDateTimeOrder", tomorrow),
                   Builders<Aggregate.Order>.Filter.Nin("OrderCode", excludedOrderCodes)
               );

                var result = await _context.OrdersCollection
                    .Find(filter)
                    .Sort("{CreateDateTimeOrder:-1 , \"OrderStatus._id\":-1 ,\"Delivery.IsScheduled\": -1}")
                    .FirstOrDefaultAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetOrdersHistory() Exception: {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<VendorType>> GetVendors()
        {
            IDbConnection _connection = new SqlConnection(_config.GetConnectionString(ConnectionString));
            var result =
                await _connection.QueryAsync<VendorType>(
                    $"SELECT VENDORCODE VendorCode , VENDORNAME VendorName from [vendor].[VENDOR] ");
            return result.ToList();
            ;
        }
        public async Task<Vendor> GetVendorByVendorCode(string vendorCode)
        {
            IDbConnection _connection = new SqlConnection(_config.GetConnectionString(ConnectionString));
            var result =
                await _connection.QueryFirstOrDefaultAsync<Vendor>(
                    $"SELECT * FROM [vendor].[VENDOR] WHERE VendorCode = @VendorCode", new { VendorCode = vendorCode });
            return result;
        }

        public async Task<PagedData<Aggregate.Order>> Tenders(string orderCode)
        {
            try
            {
                var filter = Builders<Aggregate.Order>.Filter.And(
                    Builders<Aggregate.Order>.Filter.Eq("OrderCode", orderCode),
                    Builders<Aggregate.Order>.Filter.Or(
                          Builders<Aggregate.Order>.Filter.Eq("OrderStatus._id", OrderStatus.APay.Id),
                          Builders<Aggregate.Order>.Filter.Eq("OrderStatus._id", OrderStatus.NFC.Id)
                        )
                );

                var projection = Builders<Aggregate.Order>.Projection.Exclude("Customer");

                var result = _context.OrdersCollection.Find(filter).Project<Aggregate.Order>(projection).ToList();
                //foreach (var item in result)
                //{
                //    var products = item.OrderDetails.Select(a => a.IRC);
                //    var imageData = await fetchImageProduct(products.ToList());
                //    if (imageData != null)
                //    {
                //        foreach (var item1 in imageData)
                //        {
                //            item.OrderDetails.Find(a => a.IRC == item1.IRC).ImageLink = item1.ImageLink;
                //        }
                //    }
                //}

                return new PagedData<Aggregate.Order>
                {
                    QueryResult = result.OrderBy(a => a.FinalPrice).ToList(),
                    TotalCount = 5,
                    PageSize = 1,
                    PageNumber = 10
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Tenders Exception: {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<string>> TenderVendors(double latitude, double longitude, bool needAllCityVendors)
        {
            dynamic model = new
            {
                Latitude = latitude,
                Longitude = longitude,
                NeedAllCityVendors = needAllCityVendors
            };

            HttpClient client = new HttpClient();
            var json = JsonConvert.SerializeObject(model,
                new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var result = await _fallbackPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(() =>
                _circuitBreaker.ExecuteAsync(() =>
                    client.PostAsync(this._config.GetSection("Tender").GetSection(_findVendor).Value, content)
                )));
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var responseMessage = await result.Content.ReadAsStringAsync();

                var obj = JsonConvert.DeserializeObject<DTOs.VendorModel>(responseMessage);
                List<string> vendorCodes = obj.Vendors.Select(v => v.VendorCode).ToList();
                return vendorCodes;
            }

            return null;
        }

        public async Task<List<string>> TenderVendorsOverNight(double latitude, double longitude, string grade, double radius)
        {
            dynamic model = new
            {
                Latitude = latitude,
                Longitude = longitude,
                Grade = grade,
                Radius = radius
            };
            HttpClient client = new HttpClient();
            var json = JsonConvert.SerializeObject(model,
                new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var result = await _fallbackPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(() =>
                _circuitBreaker.ExecuteAsync(() =>
                    client.PostAsync(this._config.GetSection("Tender").GetSection(_findVendor).Value, content)
                )));
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var responseMessage = await result.Content.ReadAsStringAsync();

                var obj = JsonConvert.DeserializeObject<DTOs.VendorModel>(responseMessage);
                List<string> vendorCodes = obj.Vendors.Where(x => x.IsAllTime != true).Select(v => v.VendorCode).ToList();
                return vendorCodes;
            }

            return null;
        }

        public async Task<Aggregate.Order> GetOrderDraft(string phoneNumber, string orderCode)
        {
            try
            {
                int[] status = [OrderStatus.Draft.Id,
                    OrderStatus.Ack.Id,
                    OrderStatus.APay.Id,
                    OrderStatus.Pick.Id,
                    OrderStatus.NFC.Id,
                    OrderStatus.Accept.Id,
                    OrderStatus.ADelivery.Id,
                    OrderStatus.SendDelivery.Id ,
                    OrderStatus.Deliverd.Id,
                    OrderStatus.CancelCustomer.Id,
                    OrderStatus.Reject.Id];

                var filter = Builders<Aggregate.Order>.Filter.And(
                    Builders<Aggregate.Order>.Filter.Eq("OrderCode", orderCode),
                    Builders<Aggregate.Order>.Filter.In("OrderStatus._id", status)
                );

                var result = _context.OrdersCollection.Find(filter).ToList();

                if (result.Any() && (result.Select(x => x.Customer.PhoneNumber).FirstOrDefault()) != phoneNumber)
                    throw new UnauthorizedAccessException();

                result = result.OrderByDescending(a => a.OrderStatus.Id).ToList();

                if (result.Count > 1)
                {
                    result.RemoveAll(x => string.IsNullOrEmpty(x.VendorCode));

                    if (result.All(x => x.OrderStatus.Id == OrderStatus.CancelVendor.Id))
                        foreach (var order in result.Where(x => x.OrderStatus.Name == OrderStatus.CancelVendor.Name))
                        {
                            order.DeclineType = DeclineType.CancelOrderbyPharmecy;
                        }
                    if (result.All(x => x.OrderStatus.Id == OrderStatus.Ack.Id))
                        result.FirstOrDefault(x => x.Delivery.IsScheduled);
                    else
                        result.RemoveAll(x => x.OrderStatus.Name == OrderStatus.CancelVendor.Name);
                }

                return result.FirstOrDefault();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError($"Tenders Exception: {ex.Message}");
                throw new UnauthorizedAccessException(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Tenders Exception: {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        private async Task<List<ProductResultDto>> fetchImageProduct(List<string> irc)
        {
            HttpClient client = new HttpClient();
            string uri = this._config.GetSection("ProductManagement").GetSection(_fetchImageProducts).Value +
                         $"?irc={string.Join(",", irc)}";
            var result = await _fallbackPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(() =>
                _circuitBreaker.ExecuteAsync(() =>
                    client.GetAsync(uri)
                )));
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var responseMessage = await result.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<List<ProductResultDto>>(responseMessage);
                return obj;
            }
            else
                return null;
        }

        public async Task<decimal> GetDeliveryPrice(Aggregate.Order order)
        {
            if (order.IsSpecialPatient || order.VendorCode == "V00001")
                return 0;
            else
            {

                return 297000;
                //DeliverypriceDto deliveryprice = new()
                //{
                //    Latitude = order.Customer.Addresses.First().Latitude,
                //    Longitude = order.Customer.Addresses.First().Longitude,
                //    Address = order.Customer.Addresses.First().ValueAddress,
                //    VendorCode = order.VendorCode,
                //    ParcelValue = 200000
                //};

                //HttpClient client = new HttpClient();
                //var json = JsonConvert.SerializeObject(deliveryprice,
                //    new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                //var content = new StringContent(json, Encoding.UTF8, "application/json");
                //var result = await _fallbackPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(() =>
                //    _circuitBreaker.ExecuteAsync(() =>
                //        client.PostAsync(this._config.GetSection("Delivery").GetSection(_priceInquiryOnDemand).Value,
                //            content)
                //    )));
                //if (result.StatusCode == System.Net.HttpStatusCode.OK)
                //{
                //    var responseMessage = await result.Content.ReadAsStringAsync();
                //    var obj = JsonConvert.DeserializeObject<ResponseDeliveryPrice>(responseMessage);
                //    if (obj.IsSuccess)
                //    {
                //        return obj.Data.FirstOrDefault().TotalDeliveryCost / 2;
                //    }

                //    return 40000 / 2;
                //}
                //else
                //    return 40000 / 2;
            }
        }

        public async Task<Aggregate.Order> GetOrderDraftByUser(string phoneNumber)
        {
            var filter = Builders<Aggregate.Order>.Filter.And(
                Builders<Aggregate.Order>.Filter.Eq("Customer.PhoneNumber", phoneNumber),
                Builders<Aggregate.Order>.Filter.Eq("OrderStatus._id", OrderStatus.Draft)
            );
            var result = await _context.OrdersCollection.Find(filter).Sort("{CreateDateTimeOrder: -1}").ToListAsync();
            return result.DistinctBy(a => a.OrderCode).ToList().FirstOrDefault();
        }

        public Task<Aggregate.Order> FindOrderByVendor(string orderCode, string vendorCode)
        {
            try
            {
                var filter = Builders<Aggregate.Order>.Filter.And(
                    Builders<Aggregate.Order>.Filter.Eq("OrderCode", orderCode),
                    Builders<Aggregate.Order>.Filter.Eq("VendorCode", vendorCode)
                );
                var result = _context.OrdersCollection.Find(filter).FirstOrDefaultAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetOrdersHistory() Exception: {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<long> GetDailyOrdersCountByPhoneNumber(string phoneNumber)
        {
            var startDate = long.Parse(DateTime.Now.AddDays(-1).ToString("yyyyMMddHHmmss"));
            var endDate = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"));
            var fillter = Builders<Aggregate.Order>.Filter.And(
                Builders<Aggregate.Order>.Filter.Eq("Customer.PhoneNumber", phoneNumber)
            //Builders<Aggregate.Order>.Filter.Gte(z => z.CreateDateTimeId, startDate),
            //Builders<Aggregate.Order>.Filter.Lte(z => z.CreateDateTimeId, endDate)
            );
            var countDoc = (await _context.OrdersCollection
                .DistinctAsync(z => z.OrderCode, fillter)).ToList().Count;

            return countDoc;
        }

        public async Task<List<Aggregate.Order>> GetOrder(string orderCode)
        {
            try
            {
                var filter = Builders<Aggregate.Order>.Filter.And(
                    Builders<Aggregate.Order>.Filter.Eq("OrderCode", orderCode)
                );
                var result = await _context.OrdersCollection.FindAsync(filter);
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Tenders Exception: {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<Aggregate.Order>> GetOrderForCoupon(int[] statuses, string orderCode)
        {
            try
            {
                var filterBuilder = Builders<Aggregate.Order>.Filter;
                var filter0 = filterBuilder.Eq("OrderCode", orderCode);
                var filter1 = filterBuilder.In("OrderStatus.Id", statuses);

                var filter = filter0 & filter1;
                var sort = Builders<Aggregate.Order>.Sort.Descending(z => z.CreateDateTime);
                var result = await _context.OrdersCollection.FindAsync(filter);
                return await result.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Tenders Exception: {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<OutBoxEvent>> OutBoxEventItems()
        {
            using var _connection = new SqlConnection(_config.GetConnectionString(_sqlOutBox));
            //var orders = await _connection.QueryAsync<OutBoxEvent>($"select ou.RecId , Aggregate , Json , Token , IsProcessed , RaiseDateTime , StatusCode , ou.OrderCode , EventName , gr.Step , gr.Version   " +
            //                                                       $"from [dbo].[OutBoxEventItems] as ou " +
            //                                                       $"left join Grading as gr on ou.OrderCode = gr.OrderCode " +
            //                                                       $"where Aggregate = 'Order' and EventName = 'Order' and IsProcessed = 0 and StatusCode = 100 and Version <= 2");

            var query = @"
                SELECT RecId, [Aggregate], [Json], [token], [IsProcessed], 
                       [RaiseDateTime], [StatusCode], [OrderCode], [EventName], [ParentTraceId]
                FROM [dbo].[OutBoxEventItems]
                WHERE [Aggregate] = 'Order' 
                  AND EventName = 'Order' 
                  AND IsProcessed = 0 
                  AND StatusCode = 100";

            var orders = await _connection.QueryAsync<OutBoxEvent>(query);
            
            return orders.ToList();
        }

        public async Task<List<OutBoxEvent>> OutBoxEventItemsInLastThirtyMinutes()
        {
            IDbConnection _connection = new SqlConnection(_config.GetConnectionString(_sqlOutBox));
            //var orders = await _connection.QueryAsync<OutBoxEvent>($"SELECT * FROM [dbo].[OutBoxEventItems] WHERE Aggregate = 'Order'  AND EventName = 'Order' AND RaiseDateTime BETWEEN DATEADD(MINUTE, -30, GETDATE()) AND GETDATE()");
            var orders = await _connection.QueryAsync<OutBoxEvent>
               ($"SELECT * FROM [dbo].[OutBoxEventItems] " +
                $"WHERE Aggregate = 'Order'  AND EventName = 'Order' " +
                $"AND RaiseDateTime BETWEEN DATEADD(HOUR, -1, CAST(GETDATE() AS DATETIME)) " +
                $"AND DATEADD(MINUTE, -30, CAST(GETDATE() AS DATETIME))");
            return orders.ToList();
        }

        public async Task<List<Aggregate.Order>> GetOrderAPay(string orderCode)
        {
            try
            {
                var filter = Builders<Aggregate.Order>.Filter.And(
                    Builders<Aggregate.Order>.Filter.Eq("OrderCode", orderCode),
                    Builders<Aggregate.Order>.Filter.Eq("OrderStatus._id", OrderStatus.APay.Id)
                );
                var result = await _context.OrdersCollection.FindAsync(filter);
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Tenders Exception: {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<Aggregate.Order>> FindOrderByPhoneNumber(string phoneNumber)
        {
            try
            {
                int[] status = [OrderStatus.ADelivery.Id, OrderStatus.SendDelivery.Id, OrderStatus.Deliverd.Id];
                var filter = Builders<Aggregate.Order>.Filter.And(
                    Builders<Aggregate.Order>.Filter.Eq("Customer.PhoneNumber", phoneNumber),
                    Builders<Aggregate.Order>.Filter.In("OrderStatus._id", status)
                );
                var result = await _context.OrdersCollection.FindAsync(filter);
                var distinctOrders = result.ToList().GroupBy(order => order.OrderCode).Select(group => group.First()).ToList();
                return distinctOrders;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Tenders Exception: {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<Aggregate.Order>> FetchAuctionOrder()
        {
            try
            {
                TimeZoneInfo tehranTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time");
                DateTime nowUtc = DateTime.UtcNow;
                DateTime nowTehran = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tehranTimeZone);
                DateTime startOfToday = nowTehran.Date;
                DateTime minutesAgo = nowTehran.AddMinutes(-10);
                var filter = Builders<Aggregate.Order>.Filter.And(
                    Builders<Aggregate.Order>.Filter.Gte("CreateDateTime", startOfToday),
                    Builders<Aggregate.Order>.Filter.Lte("CreateDateTime", minutesAgo),
                    Builders<Aggregate.Order>.Filter.Gte("OrderStatus._id", OrderStatus.Auction.Id),
                    Builders<Aggregate.Order>.Filter.Lte("OrderStatus._id", OrderStatus.Deliverd.Id),
                    Builders<Aggregate.Order>.Filter.Ne("VendorCode", "V00051")
                );
                var orders = await _context.OrdersCollection.FindAsync(filter);
                var groupedOrders = orders.ToList()
                    .GroupBy(o => new { OrderCode = o.OrderCode })
                    .Select(g =>
                    {
                        var minFinalPrice = g.Min(o => o.FinalPrice);
                        var minFinalPriceOrders = g.Where(o => o.FinalPrice == minFinalPrice);
                        var minDeliveryPrice = minFinalPriceOrders.Min(o => o.Delivery.FinalPrice);
                        var bestOrders = minFinalPriceOrders.Where(o => o.Delivery.FinalPrice == minDeliveryPrice);
                        var bestOrder = bestOrders.OrderByDescending(o => o.OrderDetails.Sum(d => d.Price)).FirstOrDefault();
                        return bestOrder;
                    }).Where(o => o != null).ToList();

                return groupedOrders;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Tenders Exception: {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> FindPhoneNumber(string orderCode)
        {
            try
            {
                var filter = Builders<Aggregate.Order>.Filter.And(
                    Builders<Aggregate.Order>.Filter.Eq("OrderCode", orderCode)
                );
                var projection = Builders<Aggregate.Order>.Projection.Include("Customer.PhoneNumber");
                var result = await _context.OrdersCollection.Find(filter)
                                             .Project<Aggregate.Order>(projection).FirstOrDefaultAsync();
                return result.Customer.PhoneNumber;
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetOrdersHistory() Exception: {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public Task<Aggregate.Order> FindOrderByVendorReyall(string orderCode, string vendorCode)
        {
            try
            {
                var filter = Builders<Aggregate.Order>.Filter.And(
                    Builders<Aggregate.Order>.Filter.Eq("OrderCode", orderCode),
                    Builders<Aggregate.Order>.Filter.Eq("VendorCode", vendorCode),
                    Builders<Aggregate.Order>.Filter.Ne("FinalPrice", 0)
                );
                var result = _context.OrdersCollection.Find(filter).FirstOrDefaultAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetOrdersHistory() Exception: {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<OutBoxEvent>> OutBoxEventItemsOverNight()
        {
            IDbConnection _connection = new SqlConnection(_config.GetConnectionString(_sqlOutBox));

            var orders = await _connection.QueryAsync<OutBoxEvent>($"SELECT OrderCode FROM [dbo].[OutBoxEventItems] WHERE Aggregate = 'Order' AND EventName = 'Order' AND ((CAST(RaiseDateTime AS TIME) >= '23:00:00' AND CAST(RaiseDateTime AS TIME) <= '23:59:59') " +
                                                                   $"OR (CAST(RaiseDateTime AS TIME) >= '00:00:00' AND CAST(RaiseDateTime AS TIME) < '08:00:00')) ");

            return orders.ToList();
        }

        public async Task<List<Aggregate.Order>> GetOrdersByOrderCodes(List<string> orderCodes)
        {
            try
            {
                var filter = Builders<Aggregate.Order>.Filter.In("OrderCode", orderCodes);
                var projection = Builders<Aggregate.Order>.Projection.Exclude(u => u.Id);
                var result = await _context.OrdersCollection.Find(filter).Project<Aggregate.Order>(projection).ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetOrderByOrderCode() OrderCodes {orderCodes}: exception = {ex.Message}");
                throw new ArgumentException($"Exception  order codes = {orderCodes} with error message = {ex.Message}");
            }
        }

        public async Task<List<string>> FindCurrentDayOrders()
        {
            var twoDaysAgo = DateTime.Now.AddDays(-5);
            var twoDaysAgoString = twoDaysAgo.ToString("MM/dd/yyyy");

            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Regex("CreateDateTime", new BsonRegularExpression("^" + twoDaysAgoString)),
                Builders<BsonDocument>.Filter.Eq("OrderStatus._id", BsonNull.Value)
            );

            var projection = Builders<BsonDocument>.Projection.Include("OrderCode").Exclude("_id");

            var cursor = await _context.Collection.Find(filter).Project(projection).ToListAsync();

            var orderCodes = cursor
                .Select(doc => doc.GetValue("OrderCode").AsString)
                .Distinct()
                .ToList();

            return orderCodes;
            //var todayString = DateTime.Now.ToString("MM/dd/yyyy");

            //var filter = Builders<BsonDocument>.Filter.And(
            //    Builders<BsonDocument>.Filter.Regex("CreateDateTime", new BsonRegularExpression("^" + todayString)),
            //    Builders<BsonDocument>.Filter.Eq("OrderStatus._id", BsonNull.Value)
            //);

            //var projection = Builders<BsonDocument>.Projection.Include("OrderCode").Exclude("_id");

            //var cursor = await _context.Collection.Find(filter).Project(projection).ToListAsync();

            //var orderCodes = cursor
            //    .Select(doc => doc.GetValue("OrderCode").AsString)
            //    .Distinct()
            //    .ToList();

            //return orderCodes;
        }

        public async Task<List<CancelOrder>> FindCancelOrdersLog(List<string> orderCodes)
        {
            IDbConnection _connection = new SqlConnection(_config.GetConnectionString(LoggDataCnn));
            var codes = string.Join(",", orderCodes);
            var result = await _connection.QueryAsync<CancelOrder>($"select * " +
                $"from LogCancelCustomer " +
                $"where OrderCode in " +
                $"( select n.r.value('.', 'nvarchar(MAX)') value " +
                $"from (select cast('<r>'+replace('{codes}', ',', '</r><r>')+'</r>' as xml)) as s(XMLCol) " +
                $"cross apply s.XMLCol.nodes('r') as n(r))");
            return result.ToList();
        }

        public async Task<List<Aggregate.Order>> GetAckOrders(List<string> orderCodes)
        {
            try
            {
                var blockedStatuses = new[]
                    {
                        OrderStatus.Pick.Id,
                        OrderStatus.CancelCustomer.Id,
                        OrderStatus.APay.Id,
                        OrderStatus.Deliverd.Id,
                        OrderStatus.ADelivery.Id,
                        OrderStatus.Reject.Id,
                        OrderStatus.SendDelivery.Id
                    };

                var filter = Builders<Aggregate.Order>.Filter.And(
                    Builders<Aggregate.Order>.Filter.In(o => o.OrderCode, orderCodes),
                    Builders<Aggregate.Order>.Filter.Ne(o => o.VendorCode, ""),
                    Builders<Aggregate.Order>.Filter.Nin(o => o.OrderStatus.Id, blockedStatuses)
                );

                var orders = await _context.OrdersCollection.Find(filter).ToListAsync();

                var finalOrders = orders
                    .GroupBy(o => o.OrderCode)
                    .Where(group =>
                    {
                        int total = group.Count();
                        int cancelCount = group.Count(o => o.OrderStatus.Id == OrderStatus.CancelVendor.Id);
                        return total == 0 || ((double)cancelCount / total) < 0.8;
                    })
                    .SelectMany(group => group)
                    .ToList();

                return finalOrders;


                //var filteredOrders = new List<Aggregate.Order>();
                //var finalOrders = new List<Aggregate.Order>();
                //FilterDefinition<Aggregate.Order> filter = null;

                //foreach (var orderCode in orderCodes)
                //{
                //    filter = Builders<Aggregate.Order>.Filter.And(
                //         Builders<Aggregate.Order>.Filter.Eq("OrderCode", orderCode),
                //         Builders<Aggregate.Order>.Filter.Ne("VendorCode", ""));

                //    var orders = await _context.OrdersCollection.Find(filter).ToListAsync();

                //    filteredOrders = orders
                //                    .GroupBy(o => o.OrderCode)
                //                    .Where(group => !group.Any(o => o.OrderStatus.Id == OrderStatus.Pick.Id) &&
                //                    !group.Any(o => o.OrderStatus.Id == OrderStatus.CancelCustomer.Id) &&
                //                    !group.Any(o => o.OrderStatus.Id == OrderStatus.APay.Id) &&
                //                    !group.Any(o => o.OrderStatus.Id == OrderStatus.Deliverd.Id) &&
                //                    !group.Any(o => o.OrderStatus.Id == OrderStatus.ADelivery.Id) &&
                //                    !group.Any(o => o.OrderStatus.Id == OrderStatus.Auction.Id) &&
                //                    !group.Any(o => o.OrderStatus.Id == OrderStatus.Reject.Id) &&
                //                    !group.Any(o => o.OrderStatus.Id == OrderStatus.SendDelivery.Id))
                //                    .Where(group =>
                //                    {
                //                        int total = group.Count();
                //                        int cancelCount = group.Count(o => o.OrderStatus.Id == OrderStatus.CancelVendor.Id);
                //                        return total == 0 || ((double)cancelCount / total) < 0.8;
                //                    })
                //                    .SelectMany(group => group)
                //                    .ToList();

                //    finalOrders.AddRange(filteredOrders);
                //}
                //return finalOrders;
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetAckOrders() OrderCodes {orderCodes}: exception = {ex.Message}");
                throw new ArgumentException($"Exception order codes are = {orderCodes} with error message = {ex.Message}");
            }
        }


        //public async Task<List<VendorType>> GetAllVendors()s
        //{
        //    IDbConnection _connection = new SqlConnection(_config.GetConnectionString(ConnectionString));
        //    var result =
        //        await _connection.QueryAsync<VendorType>(
        //            $"");
        //    return result.ToList();
        //}
    }
}

