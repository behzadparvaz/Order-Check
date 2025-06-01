using Amazon.Runtime.Internal.Transform;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Polly;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Retry;
using System.Net.Http.Formatting;
using System.Text;
using TapsiDOC.Order.Core.Domain.Contracts;
using TapsiDOC.Order.Core.Domain.Orders.Entities;

namespace TapsiDOC.Order.Infra.Connection.ThirdParty.Services
{
    public class OrderSmsSender : IOrderSmsSender
    {
        private const string _sendSms = "SendSms";
        private readonly Dictionary<OrderStatus, int> _statusTemplateIds = new Dictionary<OrderStatus, int>
        {
            {OrderStatus.Draft, 906937 },
            {OrderStatus.Ack, 975266 },
            {OrderStatus.APay, 400989 },
            {OrderStatus.NFC , 291535},
            {OrderStatus.Reject , 481217}
        };
        private const string customerPrefix = "تپسی دکتری";

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

        private readonly TimeSpan _startTime;
        private readonly TimeSpan _endTime;

        public OrderSmsSender(IConfiguration config)
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

            _startTime = TimeSpan.Parse(
              config["PharmecyWorkTime:Starttime"] ??
              throw new ArgumentNullException("PHARMECY WORK TIME NOT FOUND"));

            _endTime = TimeSpan.Parse(
                config["PharmecyWorkTime:EndTime"] ??
                throw new ArgumentNullException("PHARMECY WORK TIME NOT FOUND"));
        }

        public async Task SendNotification(Core.Domain.Orders.Entities.Order order)
        {
            var templateId = _statusTemplateIds[order.OrderStatus];
            var parameters = new Dictionary<string, string>() { { "ORDERCODE", order.OrderCode }, { "DECLINETYPE", order.DeclineType.Name } };

            dynamic sendSmsCommand = new
            {
                PhoneNumber = order.Customer.PhoneNumber,
                TemplateId = templateId,
                Parameters = parameters
            };

            HttpClient client = new HttpClient();
            var json = JsonConvert.SerializeObject(sendSmsCommand, new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var result = await _fallbackPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(() =>
                                                                  _circuitBreaker.ExecuteAsync(() =>
                                                                   client.PostAsync(_config.GetSection("Notification").GetSection(_sendSms).Value, content)
                                                                      )));
        }

        public async Task SendNotification(Core.Domain.Orders.Entities.Order order, List<string> chunks, string uriConnection)
        {
            var currentTime = DateTime.Now.TimeOfDay;
            bool isPreOrder = !(currentTime >= _startTime && currentTime <= _endTime);
            var preOrderTemplateId = 825817;

            var preOrderMustApply = isPreOrder && (order.OrderStatus == OrderStatus.Draft || order.OrderStatus == OrderStatus.Ack);
            var templateId = preOrderMustApply ? preOrderTemplateId : _statusTemplateIds[order.OrderStatus];
            var parameters = new Dictionary<string, string>();

            var title = string.IsNullOrEmpty(order.Customer.Name?.Trim()) ? customerPrefix : order.Customer.Name;
            var finalTitle = $"{title} عزیز";

            if (chunks != null)
            {
                int i = 0;
                parameters.Add("OrderCode", order.OrderCode);
                foreach (var chunk in chunks)
                {
                    if (i == 0)
                        parameters.Add($"DESCRIPTION", chunk);
                    else
                        parameters.Add($"DESCRIPTION{i + 1}", chunk);
                    i++;
                }
            }
            else
               if (order.OrderStatus != OrderStatus.NFC)
                parameters = new Dictionary<string, string> { { "OrderCode", order.OrderCode }, { "DESCRIPTION", order.Comment } };

            if (order.OrderStatus == OrderStatus.NFC)
                parameters = new Dictionary<string, string> { { "OrderCode", order.OrderCode }, { "SESSIONTOKEN", uriConnection } };

            parameters.Add("TITLE", finalTitle);

            dynamic sendSmsCommand = new
            {
                PhoneNumber = string.IsNullOrEmpty(order.Customer.AlternateRecipientMobileNumber) ? order.Customer.PhoneNumber : order.Customer.AlternateRecipientMobileNumber,
                TemplateId = templateId,
                Parameters = parameters
            };

            HttpClient client = new HttpClient();
            var json = JsonConvert.SerializeObject(sendSmsCommand, new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var result = await _fallbackPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(() =>
                                                                  _circuitBreaker.ExecuteAsync(() =>
                                                                   client.PostAsync(_config.GetSection("Notification").GetSection(_sendSms).Value, content)
                                                                      )));
        }

        public async Task SendNotificationScheduledOrder(Core.Domain.Orders.Entities.Order order)
        {
            var templateId = 417319;
            var parameters = new Dictionary<string, string>() { { "ORDERCODE", order.OrderCode }};

            dynamic sendSmsCommand = new
            {
                PhoneNumber = order.Customer.PhoneNumber,
                TemplateId = templateId,
                Parameters = parameters
            };

            HttpClient client = new HttpClient();
            var json = JsonConvert.SerializeObject(sendSmsCommand, new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var result = await _fallbackPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(() =>
                                                                  _circuitBreaker.ExecuteAsync(() =>
                                                                   client.PostAsync(_config.GetSection("Notification").GetSection(_sendSms).Value, content)
                                                                      )));
        }
    }

    public class MessageDto
    {
        public int Id { get; set; }
        public string? Text { get; set; }
    }
}
