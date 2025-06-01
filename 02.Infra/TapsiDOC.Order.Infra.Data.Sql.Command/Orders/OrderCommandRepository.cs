using BuildingBlock.Messaging.Command;
using Dapper;
using MassTransit;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Core.WireProtocol.Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Polly;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Retry;
using Serilog.Context;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http.Formatting;
using System.Text;
using TapsiDOC.Order.Core.Domain.Contracts;
using TapsiDOC.Order.Core.Domain.Orders.Entities;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;
using TapsiDOC.Order.Infra.Data.Sql.Commands.Orders.Context;
using TapsiDOC.Order.Infra.Data.Sql.Commands.Orders.ContextSetting;
using TapsiDOC.Order.Infra.Data.Sql.Commands.Orders.DTOs;
using MyEntities = TapsiDOC.Order.Core.Domain.Orders.Entities;

namespace TapsiDOC.Order.Infra.Data.Sql.Commands.Orders
{
    public class OrderCommandRepository : IOrderCommandRepository
    {
        private readonly OrderContext _context = null;
        private readonly IOptions<Settings> _settings;
        private readonly IConfiguration _config;
        private readonly ILogger<OrderCommandRepository> _logger;
        private readonly IEventStoreRepository _eventStore;
        private string _createOrderOMS = "CreateInsuranceOMS";
        private string _createOrderAck = "CreateOrderAck";
        private string LoggerCnn = "loggerConnectionString";
        private string LoggDataCnn = "loggDataConnectionString";
        private string _pickState = "PickState";
        private string _cancelCustomerState = "CancelCustomerState";
        private string _cancelOrderInLine = "CancelOrderInLine";
        private string _reject = "Reject";
        private string _payment = "PaymentRequest";
        private string _verifyPayment = "VerifyPayment";
        private string _deliverySchedule = "DeliverySchedule";
        private string _deliveryOnDemand = "DeliveryOnDemand";
        private string _deliveryAlopeykSchedule = "AlopeykSchedule";
        private string _deleteBasket = "DeleteBasket";
        private string _createTender = "CreateTender";
        private string _getVendor = "GetVendor";
        private const string _sqlOutBox = "SqlOutBox";
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
        private readonly AsyncFallbackPolicy<HttpResponseMessage> _fallbackPolicy;

        private static readonly ActivitySource _activitySource = new ActivitySource("orderscheduler-service");

        private readonly IServiceProvider _serviceProvider;


        
        private readonly HttpClient _httpClient;
        
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

        public OrderCommandRepository(IOptions<Settings> settings,
            IConfiguration config,
            ILogger<OrderCommandRepository> logger,
            IEventStoreRepository eventStore,
            IServiceProvider serviceProvider,
            IHttpClientFactory factory)
        {
            _context = new OrderContext(settings);
            _settings = settings;
            _config = config;
            _logger = logger;
            this._eventStore = eventStore;
            _retryPolicy = Policy
                .HandleResult<HttpResponseMessage>(result => !result.IsSuccessStatusCode)
                .RetryAsync(1, (d, c) =>
                {
                    string a = "Retry";
                });

            _fallbackPolicy = Policy.HandleResult<HttpResponseMessage>(result => (int)result.StatusCode == 400)
                .Or<BrokenCircuitException>()
                .FallbackAsync(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
                {
                    Content = new ObjectContent(typeof(MessageDto), new MessageDto
                    {
                        Id = 100,
                        Text = "Retry Send Data"
                    }, new JsonMediaTypeFormatter())
                });
            _httpClient = factory.CreateClient();
            _serviceProvider = serviceProvider;
        }

        public async Task CreateOrder(MyEntities.Order order)
        {
            try
            {
                order.Id = Guid.NewGuid().ToString();
                await _context.OrdersCollection.InsertOneAsync(order);
                await LoggerOrder(order).ConfigureAwait(false);
                var eventData = await SaveEventsAsync(order.Id, order, 1).ConfigureAwait(false);
                await RaiseEventDraftInsurance(order).ConfigureAwait(false);
                await UpdateRaiseEventDateTime(eventData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CreateOrder(): Exception = {ex.Message}");
                throw;
            }
        }

        public async Task UpdateStatus(MyEntities.Order order)
        {
            _logger.LogDebug("UpdateStatus started");

            var filter = Builders<MyEntities.Order>.Filter.And(
                Builders<MyEntities.Order>.Filter.Eq("OrderCode", order.OrderCode)
            );

            if (order.OrderStatus.Id != OrderStatus.CancelCustomer.Id && order.OrderStatus.Id != OrderStatus.Reject.Id)
            {
                filter = Builders<MyEntities.Order>.Filter.And(
                    filter,
                    Builders<MyEntities.Order>.Filter.Eq("VendorCode", order.VendorCode)
                );
            }

            var update = Builders<MyEntities.Order>.Update.Set("OrderStatus._id", order.OrderStatus.Id)
                .Set("OrderStatus.Name", order.OrderStatus.Name)
                .Set("FinalPrice", order.FinalPrice)
                .Set("TotalPrice", order.TotalPrice)
                .Set("SupervisorPharmacyPrice", order.SupervisorPharmacyPrice)
                .Set("PackingPrice", order.PackingPrice)
                .Set("Delivery.DeliveryPrice", order.Delivery.DeliveryPrice)
                .Set("Delivery.FinalPrice", order.Delivery.FinalPrice)
                .Set("Delivery.Discount.Amount", order.Delivery.Discount.Amount)
                .Set("Delivery.Discount.Percentage", order.Delivery.Discount.Percentage)
                .Set("DeclineType._id", order.DeclineType.Id)
                .Set("DeclineType.Name", order.DeclineType.Name)
                .Set("OrderDetails", order.OrderDetails)
                .Set("CancelReason", order.CancelReason)
                .Set("VendorComment", order.VendorComment);

            await _context.OrdersCollection.UpdateManyAsync(filter, update, new UpdateOptions { IsUpsert = false });
            if (order.OrderDetails.Count() != 0)
            {
                for (int i = 0; i < order.OrderDetails.Count(); i++)
                {
                    var filter1 = Builders<MyEntities.Order>.Filter.And(
                        Builders<MyEntities.Order>.Filter.Eq("OrderCode", order.OrderCode),
                        Builders<MyEntities.Order>.Filter.Eq("VendorCode", order.VendorCode),
                        Builders<MyEntities.Order>.Filter.ElemMatch(e => e.OrderDetails,
                            e => e.IRC == order.OrderDetails[i].IRC)
                    );

                    var update1 = Builders<MyEntities.Order>.Update
                        .Set(e => e.OrderDetails[i].Price, order.OrderDetails[i].Price)
                        .Set(e => e.OrderDetails[i].Quantity, order.OrderDetails[i].Quantity)
                        .Set(e => e.OrderDetails[i].ProductName, order.OrderDetails[i].ProductName)
                        .Set(e => e.OrderDetails[i].ImageLink, order.OrderDetails[i].ImageLink)
                        .Set(e => e.OrderDetails[i].Unit, order.OrderDetails[i].Unit)
                        .Set(e => e.OrderDetails[i].DoctorInstruction, order.OrderDetails[i].DoctorInstruction)
                        .Set(e => e.OrderDetails[i].ExpirationDate, order.OrderDetails[i].ExpirationDate);

                    await _context.OrdersCollection.UpdateOneAsync(filter1, update1,
                        new UpdateOptions { IsUpsert = false });

                    foreach (var item1 in order.OrderDetails[i].Alternatives)
                    {
                        var filter2 = Builders<MyEntities.Order>.Filter.And(
                            Builders<MyEntities.Order>.Filter.Eq("OrderCode", order.OrderCode),
                            Builders<MyEntities.Order>.Filter.Eq("VendorCode", order.VendorCode),
                            Builders<MyEntities.Order>.Filter.ElemMatch(e => e.OrderDetails,
                                e => e.IRC == order.OrderDetails[i].IRC)
                        );

                        var update2 = Builders<MyEntities.Order>.Update
                            .Set(e => e.OrderDetails[i].Alternatives[0].Price, item1.Price)
                            .Set(e => e.OrderDetails[i].Alternatives[0].ProductName, item1.ProductName)
                            .Set(e => e.OrderDetails[i].Alternatives[0].BrandName, item1.BrandName)
                            .Set(e => e.OrderDetails[i].Alternatives[0].Unit, item1.Unit)
                            .Set(e => e.OrderDetails[i].Alternatives[0].ProductType, item1.ProductType)
                            .Set(e => e.OrderDetails[i].Alternatives[0].Description, item1.Description)
                            .Set(e => e.OrderDetails[i].Alternatives[0].ImageLink, item1.ImageLink)
                            .Set(e => e.OrderDetails[i].Alternatives[0].Quantity, item1.Quantity)
                            .Set(e => e.OrderDetails[i].Alternatives[0].ExpirationDate, item1.ExpirationDate);
                        await _context.OrdersCollection.UpdateOneAsync(filter2, update2,
                            new UpdateOptions { IsUpsert = false });
                    }
                }
            }

            var eventStore = await SaveEventsAsync(order.Id, order, 1);
            await LoggerOrder(order);

            //if (order.OrderStatus.Id == OrderStatus.Pick.Id)
            //{
            //    await RaiseEventPick(eventStore).ConfigureAwait(false);
            //    //await RaiseEventDelivery(order).ConfigureAwait(false);
            //    await UpdateRaiseEventDateTime(eventStore).ConfigureAwait(false);

            //    _logger.LogDebug("UpdateStatus RaiseEventPick raised");
            //    //IDbConnection _connection = new SqlConnection(_config.GetConnectionString(_sqlOutBox));
            //    //var json = JsonConvert.SerializeObject(eventStore,
            //    //                new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            //    //await _connection.ExecuteAsync($"EXEC Prc_Create_OutBoxEvent @Aggregate = 'Order' , @Json = N'{json}' , " +
            //    //      $"@Token = '' , @IsProcessed = 0 , @StatusCode = {((int)HttpStatusCode.Continue)}");
            //}

            if (order.OrderStatus.Id == OrderStatus.ADelivery.Id)
            {
                string[] royals = { "V000164", "V000170", "" };
                if ((royals.Contains(order.VendorCode) && !string.IsNullOrEmpty(order.FromDeliveryTime)) || order.Delivery.IsScheduled) //if (order.VendorCode == "V00051" && !string.IsNullOrEmpty(order.FromDeliveryTime))
                {
                    await RaiseAlopeykSchedule(order);
                }
                else
                {
                    await RaiseEventDelivery(order);
                }

                await UpdateRaiseEventDateTime(eventStore);

                _logger.LogDebug("UpdateStatus RaiseEventDelivery raised");
            }

            //if (order.OrderStatus.Id == OrderStatus.CancelCustomer.Id)
            //{
            //    await RaiseEventCancelCustomer(eventStore).ConfigureAwait(false);
            //    await UpdateRaiseEventDateTime(eventStore).ConfigureAwait(false);
            //    await RaiseEventCancelOrderInLine(eventStore).ConfigureAwait(false);

            //    _logger.LogDebug("UpdateStatus EventCancelCustomer raised");
            //}

            if (order.OrderStatus.Id == OrderStatus.Reject.Id)
            {
                await RaiseEventReject(eventStore);
                await UpdateRaiseEventDateTime(eventStore);
                _logger.LogDebug("UpdateStatus EventReject raised");
            }

            if (order.OrderStatus.Id == OrderStatus.SendDelivery.Id)
            {
                await RaiseEvent(eventStore, _config["OMS:UpdateOrderStatusUrl"]);
                await UpdateRaiseEventDateTime(eventStore);
                _logger.LogDebug("UpdateStatus SendDelivery raised");
            }

            if (order.OrderStatus.Id == OrderStatus.Deliverd.Id)
            {
                await RaiseEvent(eventStore, _config["OMS:UpdateOrderStatusUrl"]);
                await UpdateRaiseEventDateTime(eventStore);

                _logger.LogDebug("UpdateStatus Delivery raised");
            }

            if (order.OrderStatus.Id == OrderStatus.CancelBiker.Id)
            {
                await RaiseEvent(eventStore, _config["OMS:UpdateOrderStatusUrl"]);
                await UpdateRaiseEventDateTime(eventStore);

                _logger.LogDebug("UpdateStatus CancelBiker raised");
            }

            _logger.LogDebug("UpdateStatus Done");
        }

        public async Task<EventData> SaveEventsAsync(string aggregateId, MyEntities.Order order, int expectedVersion)
        {
            var eventStream = await _eventStore.FindByAggregateId(aggregateId);

            //if (eventStream.Count != 0)
            //    if ((expectedVersion != -1 && eventStream[^1].Version != expectedVersion))
            //        //throw new ArgumentException($"ConcurrencyException event id {aggregateId}");

            var version = expectedVersion;
            EventData eventModel = new();
            //version++;
            eventModel = eventModel.Create(aggregateId, nameof(MyEntities.Order), version, order.OrderStatus.Name,
                JsonConvert.SerializeObject(order));
            await _eventStore.SaveAysnc(eventModel);

            //var topic = "OrderCreateEvents";
            //await _eventStore.ProduceAsync(topic, eventModel);
            // await RaiseEventPick(order);
            return eventModel;
        }

        private async Task UpdateRaiseEventDateTime(EventData data)
        {
            data = data.SetRaiseDateTime(data);
            await _eventStore.UpdateRaiseEvent(data);
        }

        private async Task RaiseEventDraftInsurance(MyEntities.Order order)
        {
            HttpClient client = new HttpClient();
            CreateOrderInsuranceOMS createOrder = new()
            {
                OrderCode = order.OrderCode,
                InsuranceTypeId = order.InsuranceType.Id,
                SupplementaryInsuranceTypeId = order.InsuranceType.Id,
                comment = order.Comment,
                ReferenceNumber = order.OrderDetails.FirstOrDefault().ReferenceNumber,
                NationalCode = order.Customer.NationalCode,
                AddressValue = "",
                CustomerName = order.Customer.Name,
                IsSpecialPatient = order.IsSpecialPatient,
                CreateDateTime = order.CreateDateTimeOrder,
                VendorCode = order.Vendors.FirstOrDefault().Code,
                PhoneNumberCustomer = order.Customer.PhoneNumber
            };

            var json = JsonConvert.SerializeObject(createOrder,
                new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var result = await _fallbackPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(() =>
                _circuitBreaker.ExecuteAsync(() =>
                    client.PostAsync(this._config.GetSection("OMS").GetSection(_createOrderOMS).Value, content)
                )));
            if (result.StatusCode != System.Net.HttpStatusCode.Created)
                throw new ArgumentException(
                    $"Error Code = {result.StatusCode.ToString()} , Message = '{result.Content.ReadAsStringAsync().ToString()}'");
        }

        private async Task RaiseEventAck(MyEntities.Order order, string? parentTraceId)
        {
            HttpClient client = new HttpClient();
            IDbConnection _connection = new SqlConnection(_config.GetConnectionString(_sqlOutBox));
            List<Item> items = new List<Item>();
            foreach (var data in order.OrderDetails)
            {
                Item item = new()
                {
                    IRC = data.IRC,
                    GTIN = data.GTIN,
                    ProductName = data.ProductName,
                    Description = data.Description,
                    Quantity = data.Quantity,
                    ReferenceNumber = data.ReferenceNumber,
                    Unit = data.Unit,
                    ImageLink = data.ImageLink,
                    AttachmentId = data.AttachmentId,
                    ProductType = data.Type.Id
                };
                items.Add(item);
            }

            CreateOrderDraftModels createOrder = new()
            {
                IsScheduled = order.Delivery.IsScheduled,
                OrderCode = order.OrderCode,
                InsuranceTypeId = order.InsuranceType.Id,
                SupplementaryInsuranceTypeId = order.SupplementaryInsuranceType.Id,
                VendorCode = order.VendorCode,
                Comment = order.Comment,
                NationalCode = order.Customer.NationalCode,
                CustomerName = order.Customer.Name,
                CreateDateTime = order.CreateDateTimeOrder,
                PhoneNumber = order.Customer.PhoneNumber,
                IsSpecialPatient = order.IsSpecialPatient,
                Items = items
            };

            var json = JsonConvert.SerializeObject(createOrder,
                new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });

            var targetUrl = this._config.GetSection("OMS").GetSection(_createOrderAck).Value; 
            if (!string.IsNullOrEmpty(parentTraceId))
            {
                SetActivityTrace(parentTraceId, targetUrl!);
            }

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var result = await _fallbackPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(() =>
                _circuitBreaker.ExecuteAsync(() =>
                    _httpClient.PostAsync(targetUrl, content)
                )));

            if (result.StatusCode != System.Net.HttpStatusCode.Created)
                throw new ArgumentException(
                    $"Error Code = {result.StatusCode.ToString()} , Message = '{result.Content.ReadAsStringAsync().ToString()}'");
            await _connection.ExecuteAsync($"EXEC Prc_Create_OutBoxEvent @Aggregate = 'OMS' , @Json = N'{json}' , " +
                    $"@Token = '' , @IsProcessed = 0 , @StatusCode = {((int)HttpStatusCode.Continue)}");
        }

        private void SetActivityTrace(string parentTraceId, string targetUrl)
        {
            var traceId = ActivityTraceId.CreateFromString(parentTraceId.AsSpan()); // 32-char hex
            var spanId = ActivitySpanId.CreateRandom(); // child span for outgoing HTTP call
            var traceFlags = ActivityTraceFlags.Recorded; // or use 0 if not sampled

            var context = new ActivityContext(traceId, spanId, traceFlags);

            using var activity = _activitySource.StartActivity(
                "SendOrderToOMS",
                ActivityKind.Client,
                context
            );

            if (activity != null)
            {
                activity.SetTag("http.method", "POST");
                activity.SetTag("http.url", targetUrl);
                activity.SetTag("messaging.parent_trace_id", parentTraceId);
                Activity.Current = activity;
            }
        }

        private async Task RaiseEventPick(EventData data)
        {
            //HttpClient client = new HttpClient();
            var json = JsonConvert.SerializeObject(data,
                new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var result = await _fallbackPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(() =>
                _circuitBreaker.ExecuteAsync(() =>
                    _httpClient.PostAsync(this._config.GetSection("OMS").GetSection(_pickState).Value, content)
                )));
            if (result.StatusCode != System.Net.HttpStatusCode.Created)
                throw new ArgumentException(
                    $"Error Code = {result.StatusCode.ToString()} , Message = '{result.Content.ReadAsStringAsync().ToString()}'");
        }

        private async Task<bool> RaiseAlopeykSchedule(MyEntities.Order order)
        {
            IDbConnection _connection = new SqlConnection(_config.GetConnectionString(LoggDataCnn));
            var vendor = await GetVendor(order.VendorCode);
            //HttpClient client = new HttpClient();

            var phoneNumber = order.Customer.AlternateRecipientMobileNumber != null ? order.Customer.AlternateRecipientMobileNumber : order.Customer.PhoneNumber;

            var deliveryDate = ConvertToGregorianDate(order.Delivery.DeliveryTime);

            DeliveryModel delivery = new()
            {
                OrderCode = order.OrderCode,
                UserName = phoneNumber,
                PhoneNumber = phoneNumber,
                Latitude = order.Customer.Addresses.FirstOrDefault().Latitude,
                Longitude = order.Customer.Addresses.FirstOrDefault().Longitude,
                Address = order.Customer.Addresses.FirstOrDefault().ValueAddress,
                HouseNumber = order.Customer.Addresses.FirstOrDefault().HouseNumber,
                HomeUnit = order.Customer.Addresses.FirstOrDefault().HomeUnit,
                VendorName = vendor.VendorName,
                VendorCode = vendor.VendorCode,
                VendorLatitude = vendor.Location.Latitude,
                VendorLongitude = vendor.Location.Longitude,
                VendorPhoneNumber = "",
                VendorAddress = "",
                DeliveryDate = deliveryDate
            };

            var json = JsonConvert.SerializeObject(delivery,
                     new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var result = await _fallbackPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(() =>
                    _circuitBreaker.ExecuteAsync(() =>
                        _httpClient.PostAsync(this._config.GetSection("Delivery").GetSection(_deliveryAlopeykSchedule).Value, content)
                    )));
            var res = await result.Content.ReadAsStringAsync();
            if (result.StatusCode == HttpStatusCode.OK)
            {

                await _connection.ExecuteAsync(
                            $"EXEC PRC_LOG_DELIVERY @OrderCode = '{order.OrderCode}' , @Data = N'{json}' , @Message = 'AlopeykSchedule'");
                return true;
            }
            else
            {
                await _connection.ExecuteAsync(
                            $"EXEC PRC_LOG_DELIVERY @OrderCode = '{order.OrderCode}' , @Data = N'{json}' , @Message = '{res}'");
                return false;
            }

        }

        private static string ConvertToGregorianDate(string persianDate)
        {
            PersianCalendar persianCalendar = new PersianCalendar();

            string[] parts = persianDate.Split('/');
            int year = int.Parse(parts[0]);
            int month = int.Parse(parts[1]);
            int day = int.Parse(parts[2]);

            DateTime gregorianDate = persianCalendar.ToDateTime(year, month, day, 0, 0, 0, 0);

            return gregorianDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        private async Task<bool> RaiseEventDelivery(MyEntities.Order order)
        {
            _logger.LogDebug("RaiseEventDelivery started");
            _logger.LogDebug("RaiseEventDelivery input order: {@order}", order);

            IDbConnection _connection = new SqlConnection(_config.GetConnectionString(LoggDataCnn));
            var vendor = await GetVendor(order.VendorCode);

            _logger.LogDebug("RaiseEventDelivery after GetVendor");

            DeliveryModel delivery = new()
            {
                OrderCode = order.OrderCode,
                UserName = order.Customer.Name,
                PhoneNumber = order.Customer.AlternateRecipientMobileNumber != null ? order.Customer.AlternateRecipientMobileNumber : order.Customer.PhoneNumber,
                Latitude = order.Customer.Addresses.FirstOrDefault().Latitude,
                Longitude = order.Customer.Addresses.FirstOrDefault().Longitude,
                Address = order.Customer.Addresses.FirstOrDefault().ValueAddress,
                HouseNumber = order.Customer.Addresses.FirstOrDefault().HouseNumber,
                HomeUnit = order.Customer.Addresses.FirstOrDefault().HomeUnit,
                VendorName = vendor.VendorName,
                VendorCode = vendor.VendorCode,
                VendorLatitude = vendor.Location.Latitude,
                VendorLongitude = vendor.Location.Longitude,
                VendorPhoneNumber = "",
                VendorAddress = "",
                DeliveryDate = order.Delivery.DeliveryTime
            };

            _logger.LogDebug("RaiseEventDelivery Delivery model: {@delivery}", delivery);

            //HttpClient client = new HttpClient();
            string subRoot = string.Empty;
            var json = JsonConvert.SerializeObject(delivery,
                new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });

            _logger.LogDebug("RaiseEventDelivery before executing PRC_LOG_DELIVERY");

            await _connection.ExecuteAsync(
                $"EXEC PRC_LOG_DELIVERY @OrderCode = '{order.OrderCode}' , @Data = N'{json}' , @Message = N'Send Delivery OnDemand'");

            _logger.LogDebug("RaiseEventDelivery after executing PRC_LOG_DELIVERY");

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            if (order.IsSpecialPatient)
                subRoot = _deliverySchedule;
            else
            {
                var sendEndpointProvider = _serviceProvider.GetRequiredService<ISendEndpointProvider>();
                //subRoot = _deliveryOnDemand;
                var sendEndpoint = await sendEndpointProvider.GetSendEndpoint(new Uri("exchange:order_dms_create_delivery_ondemand"));
                await sendEndpoint.Send(new CreateOrderOnDemandCommand()
                {
                    Address = delivery.Address,
                    DeliveryDate = delivery.DeliveryDate,
                    HomeUnit = delivery.HomeUnit,
                    HouseNumber = delivery.HouseNumber,
                    Latitude = delivery.Latitude,
                    Longitude = delivery.Longitude,
                    OrderCode = delivery.OrderCode,
                    PhoneNumber = delivery.PhoneNumber,
                    UserName = delivery.UserName,
                    VendorAddress = delivery.VendorAddress,
                    VendorCode = delivery.VendorCode,
                    VendorLatitude = delivery.VendorLatitude,
                    VendorLongitude = delivery.VendorLongitude,
                    VendorName = delivery.VendorName,
                    VendorPhoneNumber = delivery.VendorPhoneNumber,
                });
            }

            if (string.IsNullOrEmpty(subRoot) == false)
            {
                _logger.LogDebug("RaiseEventDelivery before calling Delivery service :{@url}", this._config.GetSection("Delivery").GetSection(subRoot).Value);

            var result = await _fallbackPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(() =>
                _circuitBreaker.ExecuteAsync(() =>
                    _httpClient.PostAsync(this._config.GetSection("Delivery").GetSection(subRoot).Value, content)
                )));

                _logger.LogDebug("RaiseEventDelivery api called: {@url}", this._config.GetSection("Delivery").GetSection(subRoot).Value);

                if (result.StatusCode != System.Net.HttpStatusCode.Created &&
                    result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    var res = await result.Content.ReadAsStringAsync();

                    _logger.LogDebug("RaiseEventDelivery api called. Error result: {@res}", res);

                    await _connection.ExecuteAsync(
                        $"EXEC PRC_LOG_DELIVERY @OrderCode = '{order.OrderCode}' , @Data = N'{json}' , @Message = N'{res}'");
                    throw new ArgumentException($"Error Code = {result.StatusCode.ToString()} , Message = '{res}'");
                }

                await _connection.ExecuteAsync(
                    $"EXEC PRC_LOG_DELIVERY @OrderCode = '{order.OrderCode}' , @Data = N'{json}' , @Message = N'Delivery OnDemand Success'");

            }
            _logger.LogDebug("RaiseEventDelivery api call Done");

            return true;
        }

        private async Task RaiseEventReject(EventData data)
        {
            //HttpClient client = new HttpClient();
            var json = JsonConvert.SerializeObject(data,
                new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var result = await _fallbackPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(() =>
                _circuitBreaker.ExecuteAsync(() =>
                    _httpClient.PostAsync(this._config.GetSection("OMS").GetSection(_reject).Value, content)
                )));
            if (result.StatusCode != System.Net.HttpStatusCode.Created)
                throw new ArgumentException(
                    $"Error Code = {result.StatusCode} , Message = '{result.Content.ReadAsStringAsync()}'");
        }

        private async Task RaiseEventCancelCustomer(EventData data)
        {
            //HttpClient client = new HttpClient();
            var json = JsonConvert.SerializeObject(data,
                new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            this._logger.LogInformation($"Cancel Customer Root : {this._config.GetSection("OMS").GetSection(_cancelCustomerState).Value}");
            var result = await _fallbackPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(() =>
                _circuitBreaker.ExecuteAsync(() =>
                    _httpClient.PostAsync(this._config.GetSection("OMS").GetSection(_cancelCustomerState).Value, content)
                )));
            if (result.StatusCode != System.Net.HttpStatusCode.Created)
                throw new ArgumentException(
                    $"Error Code = {result.StatusCode} , Message = '{result.Content.ReadAsStringAsync()}'");
        }

        private async Task RaiseEventCancelOrderInLine(EventData data)
        {
            //HttpClient client = new HttpClient();
            var json = JsonConvert.SerializeObject(data,
                new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            var orderEvent = JsonConvert.DeserializeObject<MyEntities.Order>(data.Event);
            string url = $"{this._config.GetSection("RequestOrder").GetSection(_cancelOrderInLine).Value}/{orderEvent.OrderCode}";
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            await _fallbackPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(() =>
                    _circuitBreaker.ExecuteAsync(() =>
                        _httpClient.PatchAsync(url, content)
                    )));
        }

        private async Task<string> RaiseEventPayment(string orderCode, decimal amount, string phoneNumber,
            string vendorCode)
        {
            _logger.LogDebug("RaiseEventPayment started");
            _logger.LogDebug("RaiseEventPayment orderCode: {@orderCode}", orderCode);

            PaymentRequest model = new()
            {
                OrderCode = orderCode,
                Amount = amount,
                PhoneNumber = phoneNumber,
                VendorCode = vendorCode
            };
            //HttpClient client = new HttpClient();
            var json = JsonConvert.SerializeObject(model,
                new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var result = await _fallbackPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(() =>
                _circuitBreaker.ExecuteAsync(() =>
                    _httpClient.PostAsync(this._config.GetSection("Payment").GetSection(_payment).Value, content)
                )));
            if (result.StatusCode != System.Net.HttpStatusCode.Created)
                throw new ArgumentException(
                    $"Error Code = {result.StatusCode.ToString()} , Message = '{result.Content.ReadAsStringAsync().ToString()}'");

            _logger.LogDebug("RaiseEventPayment called api: {@url}", this._config.GetSection("Payment").GetSection(_payment).Value);

            if (result.StatusCode == System.Net.HttpStatusCode.Created)
            {
                var payLink = await result.Content.ReadAsStringAsync();
                _logger.LogDebug("RaiseEventPayment api payLink:{@payLink}", payLink);

                return payLink;
            }
            else
            {
                var resultContent = result.Content.ReadAsStringAsync();
                _logger.LogDebug("RaiseEventPayment api error result:{@resultContent}", resultContent);

                throw new ArgumentException("پرداخت با خطا مواجه شد لطفا مجدد تلاش نمایید");
            }
        }

        public async Task<VendorModel> GetVendor(string vendorCode)
        {
            //HttpClient client = new HttpClient();
            var result = await _fallbackPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(() =>
                _circuitBreaker.ExecuteAsync(() =>
                    _httpClient.GetAsync(this._config.GetSection("VendorManagement").GetSection(_getVendor).Value +
                                    $"?VendorCode={vendorCode}")
                )));
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var content = await result.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<VendorModel>(content);
                return obj;
            }
            else
                throw new ArgumentException(
                    $"{result.StatusCode.ToString()}  , ErrorMassage = {result.Content.ReadAsStringAsync().ToString()}");
        }

        public async Task<string> Payment(string orderCode, string phoneNumber, decimal amount, string vendorCode)
        {
            var result = await RaiseEventPayment(orderCode, amount, phoneNumber, vendorCode);
            return result;
        }

        public Task DeliveryStatus(string orderCode, string statusName)
        {
            return null;
        }

        public async Task<PaymentData> VerifyPayment(string trackId, string orderCode)
        {
            VerifyPaymentModel model = new()
            {
                TrackId = trackId,
                OrderCode = orderCode
            };
            //HttpClient client = new HttpClient();
            var json = JsonConvert.SerializeObject(model,
                new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var result = await _fallbackPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(() =>
                _circuitBreaker.ExecuteAsync(() =>
                    _httpClient.PostAsync(this._config.GetSection("Payment").GetSection(_verifyPayment).Value, content)
                )));
            if (result.StatusCode != System.Net.HttpStatusCode.Created &&
                result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return new PaymentData
                {
                    Amount = 0,
                    DateTime = "",
                    IsSuccess = false,
                    TrackId = trackId,
                    VendorCode = ""

                };
            }
            //throw new ArgumentException($"Payment Error OrderCode = {orderCode} , TrackId = {trackId}");

            var response = await result.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<PaymentData>(response);
            return obj;
        }

        public async Task<bool> DescriptionProduct(MyEntities.Order order)
        {
            try
            {
                var filter = Builders<MyEntities.Order>.Filter.And(
                        Builders<MyEntities.Order>.Filter.Eq("OrderCode", order.OrderCode),
                        Builders<MyEntities.Order>.Filter.Eq("VendorCode", order.VendorCode)
                    );
                var update = Builders<MyEntities.Order>.Update.Set("OrderStatus._id", order.OrderStatus.Id)
                    .Set("OrderStatus.Name", order.OrderStatus.Name)
                    .Set("FinalPrice", order.FinalPrice)
                    .Set("TotalPrice", order.TotalPrice)
                    .Set("PackingPrice", order.PackingPrice)
                    .Set("SupervisorPharmacyPrice", order.SupervisorPharmacyPrice)
                    .Set("Delivery.DeliveryPrice", order.Delivery.DeliveryPrice)
                    .Set("Description.Comment", order.Description.Comment)
                    .Set("Description.Link", order.Description.Link);

                await _context.OrdersCollection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
                return true;
            }
            catch (Exception ex)
            {
                this._logger.LogError(
                    $"DescriptionProduct OrderCode = {order.OrderCode} , StatusName = {order.OrderStatus.Name} , Message = {ex.Message}");
                return false;
            }
        }
        public async Task<bool> UpdateDelivery(MyEntities.Order order)
        {
            try
            {
                var filter = Builders<MyEntities.Order>.Filter.And(
                        Builders<MyEntities.Order>.Filter.Eq("OrderCode", order.OrderCode),
                        Builders<MyEntities.Order>.Filter.Eq("VendorCode", order.VendorCode)
                    );
                var update = Builders<MyEntities.Order>.Update
                    .Set("Delivery.Discount.Percentage", order.Delivery.Discount.Percentage)
                    .Set("Delivery.Discount.Amount", order.Delivery.Discount.Amount)
                    .Set("Delivery.Discount.CouponCode", order.Delivery.Discount.CouponCode)
                    .Set("FinalPrice", order.FinalPrice)
                    .Set("Delivery.FinalPrice", order.Delivery.FinalPrice);

                await _context.OrdersCollection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
                return true;
            }
            catch (Exception ex)
            {
                this._logger.LogError(
                    $"DescriptionProduct OrderCode = {order.OrderCode} , StatusName = {order.OrderStatus.Name} , Message = {ex.Message}");
                return false;
            }
        }
        public async Task RaiseEventDeleteBasket(MyEntities.Order order, string token)
        {
            //Activity.Current.sp
            _logger.LogDebug("RaiseEventDeleteBasket started");

            // var recId = await InsertInOutbox(order, token, "Basket");

            _logger.LogDebug("RaiseEventDeleteBasket Done");

            //HttpClient client = new HttpClient();

            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", token);
            var result = await _fallbackPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(() =>
                _circuitBreaker.ExecuteAsync(() =>
                    _httpClient.DeleteAsync(this._config.GetSection("Basket").GetSection(_deleteBasket).Value)
                )));

            _logger.LogDebug("RaiseEventDeleteBasket api called: {@url}", this._config.GetSection("Basket").GetSection(_deleteBasket).Value);
            _logger.LogDebug("RaiseEventDeleteBasket api response: {@result}", result.Content.ReadAsStringAsync());


            //if (result.IsSuccessStatusCode)
            //{
            //    await UpdateOutBoxEvent(recId);
            //}
        }

        public async Task RaiseEventDeleteBasketScheduler(MyEntities.Order order, string token, long? recId)
        {
            _logger.LogDebug("RaiseEventDeleteBasketScheduler started");
            _logger.LogDebug("RaiseEventDeleteBasketScheduler order: {@order}", order);
            _logger.LogDebug("RaiseEventDeleteBasketScheduler outbox recId: {@recId}", recId);

            //HttpClient client = new HttpClient();
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", token);
            var result = await _fallbackPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(() =>
                _circuitBreaker.ExecuteAsync(() =>
                    _httpClient.DeleteAsync(this._config.GetSection("Basket").GetSection(_deleteBasket).Value)
                )));

            _logger.LogDebug("RaiseEventDeleteBasketScheduler api called: {@url}", this._config.GetSection("Basket").GetSection(_deleteBasket).Value);
            _logger.LogDebug("RaiseEventDeleteBasketScheduler response: {@result}", result.Content.ReadAsStringAsync());

            if (result.IsSuccessStatusCode && recId.HasValue)
            {
                await UpdateOutBoxEvent(recId.Value);

                _logger.LogDebug("RaiseEventDeleteBasketScheduler outbox updated");
            }

            _logger.LogDebug("RaiseEventDeleteBasketScheduler Done");
        }

        private async Task<long> InsertInOutbox(MyEntities.Order order, string token, string aggregateName)
        {
            _logger.LogDebug("InsertInOutbox started");

            IDbConnection _connection = new SqlConnection(_config.GetConnectionString(_sqlOutBox));

            _logger.LogDebug("InsertInOutbox before ExecuteScalarAsync");

            var json = JsonConvert.SerializeObject(order,
                       new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            var newRecId = await _connection.ExecuteAsync($"EXEC Prc_Create_OutBoxEvent_Grading @Aggregate = 'Order' , @Json = N'{json}' , " +
                                             $"@Token = '{token}' , @IsProcessed = 0 , @StatusCode = {((int)HttpStatusCode.Continue)} , " +
                                         $"@OrderCode = '{order.OrderCode}' , @EventName = 'Order'");
            //var newRecId = await _connection.ExecuteScalarAsync<int>(insertQuery, parameters);

            _logger.LogDebug("InsertInOutbox Done, NewRecId: {@newRecId}", newRecId);

            return newRecId;
        }

        public async Task OrderDraft(MyEntities.Order order, string token)
        {
            try
            {                
                using (Serilog.Context.LogContext.PushProperty("OrderCode", order.OrderCode))                
                {
                    _logger.LogDebug("OrderCommandRepository.OrderDraft input: {@order}", order);

                    order.Id = Guid.NewGuid().ToString();
                    await _context.OrdersCollection.InsertOneAsync(order);
                    var eventData = await SaveEventsAsync(order.Id, order, 1).ConfigureAwait(false);
                    using var _connection = new SqlConnection(_config.GetConnectionString(_sqlOutBox));
               
                    var currentTraceId = Activity.Current?.TraceId.ToHexString();

                    var json = JsonConvert.SerializeObject(order,
                                new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                    await _connection.ExecuteAsync($"EXEC Prc_Create_OutBoxEvent_Grading @Aggregate = 'Order' , @Json = N'{json}' , " +
                                                     $"@Token = '{token}' , @IsProcessed = 0 , @StatusCode = {((int)HttpStatusCode.Continue)} , " +
                                                     $"@OrderCode = '{order.OrderCode}' , @EventName = 'Order', @MaxStep = '1' , @ParentTraceId = '{currentTraceId}' ");

                    _logger.LogDebug("OrderCommandRepository.OrderDraft Prc_Create_OutBoxEvent executed");

                    await RaiseEventDeleteBasket(order, token).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("CreateOrder(): Exception = {@Message}", ex.Message);
                throw;
            }
        }

        public async Task<List<string>> CreateTender(string orderCode, double latitude, double longitude)
        {
            CreateTenderDto model = new()
            {
                OrderCode = orderCode,
                Latitude = latitude,
                Longitude = longitude
            };
            //HttpClient client = new HttpClient();
            var json = JsonConvert.SerializeObject(model,
                new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var result = await _fallbackPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(() =>
                _circuitBreaker.ExecuteAsync(() =>
                    _httpClient.PostAsync(this._config.GetSection("Tender").GetSection(_createTender).Value, content)
                )));
            if (result.StatusCode == System.Net.HttpStatusCode.Created &&
                result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var responseMessage = await result.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<List<string>>(responseMessage);
                return obj;
            }

            return null;
        }

        private async Task RaiseEvent(EventData data, string url)
        {
            //HttpClient client = new HttpClient();
            var json = JsonConvert.SerializeObject(data,
                new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var result = await _fallbackPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(() =>
                _circuitBreaker.ExecuteAsync(() =>
                    _httpClient.PostAsync(url, content)
                )));
            if (result.StatusCode != System.Net.HttpStatusCode.Created)
            {
                var resultContent = result.Content.ReadAsStringAsync().ToString();
                _logger.LogDebug("RaisEvent {@url} returned error result: {@resultContent}", url, resultContent);

                throw new ArgumentException(
                    $"Error Code = {result.StatusCode.ToString()} , Message = '{resultContent}'");
            }
        }

        private async Task LoggerOrder(MyEntities.Order order)
        {
            IDbConnection _connection = new SqlConnection(_config.GetConnectionString(LoggerCnn));
            await _connection.ExecuteAsync(
                $"EXEC PRC_LOGGER_ORDER @OrderCode = '{order.OrderCode}' , @StatusId = {order.OrderStatus.Id} , @AggregateName = 'Order' , @VendorCode = '{order.VendorCode}'");
        }

        public async Task UpdateOrderDetails(MyEntities.Order order)
        {
            var filter = Builders<MyEntities.Order>.Filter.Eq(o => o.OrderCode, order.OrderCode);
            var update = Builders<MyEntities.Order>.Update.Set("OrderDetails", order.OrderDetails)
                                                    .Set("OrderStatus.Name", order.OrderStatus.Name)
                                                    .Set("OrderStatus._id", order.OrderStatus.Id);
            await _context.OrdersCollection.UpdateManyAsync(filter, update);
            var eventData = await SaveEventsAsync(order.Id, order, 1).ConfigureAwait(false);
            // await RaiseEventAck(order).ConfigureAwait(false);
            await UpdateRaiseEventDateTime(eventData).ConfigureAwait(false);
        }

        public async Task UpdateOrderReject(MyEntities.Order order)
        {
            var filter = Builders<MyEntities.Order>.Filter.Eq(o => o.OrderCode, order.OrderCode);
            var update = Builders<MyEntities.Order>.Update.Set("Comment", order.Comment)
                                                    .Set("OrderStatus.Name", order.OrderStatus.Name)
                                                    .Set("OrderStatus._id", order.OrderStatus.Id);
            await _context.OrdersCollection.UpdateManyAsync(filter, update);
            var eventData = await SaveEventsAsync(order.Id, order, 1).ConfigureAwait(false);
            // await RaiseEventAck(order).ConfigureAwait(false);
            await UpdateRaiseEventDateTime(eventData).ConfigureAwait(false);
        }

        public async Task AssignmentOrder(MyEntities.Order order)
        {
            try
            {
                order.Id = Guid.NewGuid().ToString();
                await _context.OrdersCollection.InsertOneAsync(order);
                var eventData = await SaveEventsAsync(order.Id, order, 1).ConfigureAwait(false);
                if (order.OrderStatus == OrderStatus.Ack)
                {
                    await RaiseEventAck(order, null).ConfigureAwait(false);
                    await UpdateRaiseEventDateTime(eventData).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"CreateOrder(): Exception = {ex.Message}");
                throw;
            }
        }

        public async Task OrderScheduled(MyEntities.Order order, long updateOutBox, string parentSpanId)
        {
            try
            {
                _logger.LogDebug("OrderScheduled Input: {@order}", order);

                order.Id = Guid.NewGuid().ToString();
                await _context.OrdersCollection.InsertOneAsync(order);
                var eventData = await SaveEventsAsync(order.Id, order, 1).ConfigureAwait(false);
                IDbConnection _connection = new SqlConnection(_config.GetConnectionString(_sqlOutBox));

                await RaiseEventAck(order, parentSpanId).ConfigureAwait(false);
                await UpdateRaiseEventDateTime(eventData).ConfigureAwait(false);
                await _connection.ExecuteAsync($"update [dbo].[OutBoxEventItems] " +
                                         $"set IsProcessed = 1 , StatusCode = 200 " +
                                         $"where RecId = {updateOutBox}");

                _logger.LogDebug($"OrderScheduled Done");
            }
            catch (Exception ex)
            {
                _logger.LogError($"OrderScheduled CreateOrder(): Exception = {ex.Message}");
                throw;
            }
        }

        public async Task UpdateOutBoxEvent(long updateId)
        {
            _logger.LogDebug($"UpdateOutBoxEvent Started");
            _logger.LogDebug("UpdateOutBoxEvent Input updateId: {@updateId}", updateId);

            IDbConnection _connection = new SqlConnection(_config.GetConnectionString(_sqlOutBox));
            await _connection.ExecuteAsync($"update [dbo].[OutBoxEventItems] " +
                         $"set IsProcessed = 1 , StatusCode = 200 " +
                         $"where RecId = {updateId}");

            _logger.LogDebug("UpdateOutBoxEvent Done");
        }

        public async Task ChangeStateOrderApay(MyEntities.Order order)
        {
            var filter = Builders<MyEntities.Order>.Filter.And(
                    Builders<MyEntities.Order>.Filter.Eq(o => o.OrderCode, order.OrderCode),
                    Builders<MyEntities.Order>.Filter.Eq("VendorCode", order.VendorCode)
                );
            var update = Builders<MyEntities.Order>.Update.Set("OrderStatus.Name", order.OrderStatus.Name)
                                                    .Set("OrderStatus._id", order.OrderStatus.Id);
            await _context.OrdersCollection.UpdateManyAsync(filter, update);
        }

        public async Task UpdateDeliveryRoyal(MyEntities.Order order)
        {
            var filter = Builders<MyEntities.Order>.Filter.And(
                                    Builders<MyEntities.Order>.Filter.Eq(o => o.OrderCode, order.OrderCode),
                                    Builders<MyEntities.Order>.Filter.Eq("VendorCode", order.VendorCode)
                                   );
            var update = Builders<MyEntities.Order>.Update.Set("Delivery.FinalPrice", order.Delivery.FinalPrice)
                                                           .Set("FinalPrice", order.FinalPrice)
                                                           .Set("Delivery.DeliveryTime", order.Delivery.DeliveryTime)
                                                           .Set("FromDeliveryTime", order.FromDeliveryTime)
                                                           .Set("ToDeliveryTime", order.ToDeliveryTime);
            await _context.OrdersCollection.UpdateOneAsync(filter, update);
        }

        public async Task UpdatePickOrder(MyEntities.Order order)
        {
            var filter = Builders<MyEntities.Order>.Filter.And(
                            Builders<MyEntities.Order>.Filter.Eq("OrderCode", order.OrderCode),
                            Builders<MyEntities.Order>.Filter.Eq("VendorCode", order.VendorCode)
                        );

            var update = Builders<MyEntities.Order>.Update.Set("OrderStatus.Name", order.OrderStatus.Name)
                                                    .Set("OrderStatus._id", order.OrderStatus.Id)
                                                    .Set("PrepartionTime", order.PrepartionTime);

            await _context.OrdersCollection.UpdateOneAsync(filter, update);

            var eventStore = await SaveEventsAsync(order.Id, order, 1).ConfigureAwait(false);
            await LoggerOrder(order).ConfigureAwait(false);
            await RaiseEventPick(eventStore).ConfigureAwait(false);
            await UpdateRaiseEventDateTime(eventStore).ConfigureAwait(false);
        }

        public async Task UpdateCancelCustomer(MyEntities.Order order)
        {
            try
            {
                var filter = Builders<MyEntities.Order>.Filter.And(
                             Builders<MyEntities.Order>.Filter.Eq("OrderCode", order.OrderCode)
                               );

                // var update = Builders<MyEntities.Order>.Update.Set("OrderStatus.Name", order.OrderStatus.Name)
                //                                         .Set("OrderStatus._id", order.OrderStatus.Id)
                //                                         .Set("DeclineType.Name", order.DeclineType.Name)
                //                                         .Set("DeclineType._id", order.DeclineType.Id)
                //                                         .Set("CancelReason" , order.CancelReason);
                // await _context.OrdersCollection.UpdateManyAsync(filter, update);

                var updates = new List<UpdateDefinition<MyEntities.Order>>();
                if (order.OrderStatus != null)
                {
                    if (!string.IsNullOrEmpty(order.OrderStatus.Name))
                        updates.Add(Builders<MyEntities.Order>.Update.Set(a => a.OrderStatus.Name, order.OrderStatus.Name));
                    if (order.OrderStatus.Id != null)
                        updates.Add(Builders<MyEntities.Order>.Update.Set(a => a.OrderStatus.Id, order.OrderStatus.Id));
                }

                if (order.DeclineType != null)
                {
                    if (!string.IsNullOrEmpty(order.DeclineType.Name))
                        updates.Add(Builders<MyEntities.Order>.Update.Set(a => a.DeclineType.Name, order.DeclineType.Name));

                    if (order.DeclineType.Id != null)
                        updates.Add(Builders<MyEntities.Order>.Update.Set(a => a.DeclineType.Id, order.DeclineType.Id));
                }

                if (!string.IsNullOrEmpty(order.CancelReason))
                    updates.Add(Builders<MyEntities.Order>.Update.Set(a => a.CancelReason, order.CancelReason));

                if (updates.Count > 0)
                {
                    IDbConnection _connection = new SqlConnection(_config.GetConnectionString(LoggDataCnn));
                    await _connection.ExecuteAsync($"PRC_CANCEL_CUSTOMER @OrderCode = '{order.OrderCode}' , @StatusId = {order.OrderStatus.Id} , " +
                                                    $"@StatusName = N'{order.OrderStatus.Name}' , @DeclineId = {order.DeclineType.Id} , @DeclineName = N'{order.DeclineType.Name}'");
                    var update = Builders<MyEntities.Order>.Update.Combine(updates);
                    await _context.OrdersCollection.UpdateManyAsync(filter, update);
                }

                _logger.LogDebug("Scuccess update cancel customer Order: {@order}",
                                JsonConvert.SerializeObject(order, new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() }));

                var eventStore = await SaveEventsAsync(order.Id, order, 1).ConfigureAwait(false);
                await LoggerOrder(order).ConfigureAwait(false);
                await RaiseEventCancelCustomer(eventStore).ConfigureAwait(false);
                await UpdateRaiseEventDateTime(eventStore).ConfigureAwait(false);
                _logger.LogDebug($"UpdateStatus EventCancelCustomer raised = {order.OrderCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"exception update cancel customer Order: " +
                    $" {JsonConvert.SerializeObject(order, new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() })} , " +
                    $"exception message:{ex.Message}");
            }
        }

        public async Task<bool> AddReview(Review review)
        {
            var filter = Builders<MyEntities.Order>.Filter.And(
                Builders<MyEntities.Order>.Filter.Eq("OrderCode", review.OrderCode),
                            Builders<MyEntities.Order>.Filter.Eq("VendorCode", review.VendorCode));

            var order = await _context.OrdersCollection.FindAsync(filter);

            var findOrder = order.FirstOrDefault();
            findOrder.Reviews.Add(review);

            var update = Builders<MyEntities.Order>.Update.Set("Reviews", findOrder.Reviews)
                .Set("HasReview", true);

            await _context.OrdersCollection.UpdateOneAsync(filter, update);

            return true;
        }

        public async Task UpdateCancelCustomerV2(string orderCode, int statusId, string statusName, int declineId, string declineName)
        {
            var filter = Builders<MyEntities.Order>.Filter.And(
             Builders<MyEntities.Order>.Filter.Eq("OrderCode", orderCode)
               );

            var update = Builders<MyEntities.Order>.Update.Set("OrderStatus.Name", statusName)
                                                    .Set("OrderStatus._id", statusId)
                                                    .Set("DeclineType.Name", declineName)
                                                    .Set("DeclineType._id", declineId);
            await _context.OrdersCollection.UpdateManyAsync(filter, update);
        }
    }
}