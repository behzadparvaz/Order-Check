using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Polly;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Retry;
using System.Net.Http.Formatting;
using System.Text;
using TapsiDOC.Order.Core.Domain.Contracts;
using TapsiDOC.Order.Infra.Connection.ThirdParty.Dtos;

namespace TapsiDOC.Order.Infra.Connection.ThirdParty.Services
{
    public class OMSSender : IOMSSender
    {
        private const string _createOrderAck = "CreateOrderAck";
        private readonly IConfiguration _config;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
        private readonly AsyncFallbackPolicy<HttpResponseMessage> _fallbackPolicy;
        private readonly ILogger<OMSSender> logger;

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

        public OMSSender(IConfiguration config, ILogger<OMSSender> logger)
        {
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

            _config = config;
            this.logger = logger;
        }
        public async Task SendOms(Core.Domain.Orders.Entities.Order order)
        {
            logger.LogDebug("OMSSender.SendOms started");
            logger.LogDebug("OMSSender.SendOms input command: {@order}", order);

            HttpClient client = new HttpClient();
            var items = new List<dynamic>();
            foreach (var data in order.OrderDetails)
            {
                items.Add(new
                {
                    IRC = data.IRC,
                    GTIN = data.GTIN,
                    ProductName = data.ProductName,
                    Description = data.Description,
                    Quantity = data.Quantity,
                    ReferenceNumber = data.ReferenceNumber,
                    ImageLink = data.ImageLink,
                    ProductType = data.Type.Id
                });
            }

            dynamic createOrder = new
            {
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
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var result = await _fallbackPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(() =>
                _circuitBreaker.ExecuteAsync(() =>
                    client.PostAsync(this._config.GetSection("OMS").GetSection(_createOrderAck).Value, content)
                )));

            var resultContent = result.Content.ReadAsStringAsync().ToString();
            logger.LogDebug("RaisEvent {@url} returned error result: {@resultContent}", this._config.GetSection("OMS").GetSection(_createOrderAck).Value, resultContent);
            logger.LogDebug("RaiseEventDelivery api called: {@url}", this._config.GetSection("OMS").GetSection(_createOrderAck).Value);
            
            if (result.StatusCode != System.Net.HttpStatusCode.Created)
                throw new ArgumentException(
                    $"Error Code = {result.StatusCode.ToString()} , Message = '{result.Content.ReadAsStringAsync().ToString()}'");
        }
    }
}
