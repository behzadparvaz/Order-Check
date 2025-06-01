using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Polly;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Retry;
using System.Data;
using System.Net;
using System.Net.Http.Formatting;
using System.Text;
using TapsiDOC.Order.Core.Domain.Contracts;
using TapsiDOC.Order.Core.Domain.Orders.Entities;

namespace TapsiDOC.Order.Infra.Connection.ThirdParty.Services
{
    public class RequestOrder : IRequestOrder
    {
        private const string _requestOrder = "RequestOrderDoctor";
        private const string _sqlOutBox = "SqlOutBox";
        private readonly IConfiguration _config;
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
        private readonly ILogger<RequestOrder> logger;

        public RequestOrder(IConfiguration config,
            ILogger<RequestOrder> logger)
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

        public async Task SendRequestOrder(List<OrderDetail> orderDetails, string orderCode, string nationalCode, string phoneNumber,
                                            string fullName, string description, string token)
        {

            IDbConnection _connection = new SqlConnection(_config.GetConnectionString(_sqlOutBox));
            var parameters = new List<dynamic>();
            foreach (var item in orderDetails)
            {
                switch (item.Unit)
                {
                    case "ورق":
                        parameters.Add(new { DrugType = 1, DrugName = item.ProductName, DrugCount = (int)item.Quantity, Description = item.Description });
                        break;
                    case "شیشه":
                        parameters.Add(new { DrugType = 3, DrugName = item.ProductName, DrugCount = (int)item.Quantity, Description = item.Description });
                        break;
                    case "عدد":
                        parameters.Add(new { DrugType = 4, DrugName = item.ProductName, DrugCount = (int)item.Quantity, Description = item.Description });
                        break;
                    default:
                        parameters.Add(new { DrugType = 1, DrugName = item.ProductName, DrugCount = (int)item.Quantity, Description = item.Description });
                        break;
                }
            }

            dynamic RequestOrderModel = new
            {
                OrderCode = orderCode,
                OrderDetails = parameters,
                NationalCode = nationalCode == null ? "1050708053":nationalCode,
                FullName = fullName,
                AddressId = string.Empty,
                PhoneNumber = phoneNumber,
                Description = description
            };

            HttpClient client = new HttpClient();
            var json = JsonConvert.SerializeObject(RequestOrderModel, new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            client.DefaultRequestHeaders.Add("X-ServiceId", "TAPSIDR-5D42F31D73F04F84A7B882B4763B0A81");
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var result = await _fallbackPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(() =>
                                                                  _circuitBreaker.ExecuteAsync(() =>
                                                                   client.PostAsync(_config.GetSection("RequestOrder").GetSection(_requestOrder).Value, content)
                                                                      )));
            logger.LogDebug($"SendRequestOrder called api: RequestOrder RequestOrderDoctor");
            await _connection.ExecuteAsync($"EXEC Prc_Create_OutBoxEvent @Aggregate = 'Order' , @Json = N'{json}' , " +
                                $"@Token = '{token}' , @IsProcessed = 0 , @StatusCode = {((int)HttpStatusCode.Continue)} , " +
                                $"@OrderCode = '{orderCode}' , @EventName = 'RequestOrder'");
            if (result.StatusCode == HttpStatusCode.OK)
            {

            }

                        
            logger.LogDebug($"SendRequestOrder Done");
        }
    }
}
