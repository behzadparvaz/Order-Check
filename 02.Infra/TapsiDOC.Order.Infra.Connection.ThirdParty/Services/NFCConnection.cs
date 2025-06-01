using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
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
    public class NFCConnection : INFCConnection
    {

        private const string _joinConnection = "JoinConnection";
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

        public NFCConnection(IConfiguration config)
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
        }
        public async Task<string> JoinConnection(string meetingId, string vendorName)
        {
            dynamic sendSmsCommand = new
            {
                MeetingId = meetingId,
                Fullname = vendorName,
                Role = 1,
                AvatarURL = string.Empty
            };

            HttpClient client = new HttpClient();
            var json = JsonConvert.SerializeObject(sendSmsCommand, new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var result = await _fallbackPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(() =>
                                                                  _circuitBreaker.ExecuteAsync(() =>
                                                                   client.PostAsync(_config.GetSection("NFC").GetSection(_joinConnection).Value, content)
                                                                      )));

            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var responseMessage = await result.Content.ReadAsStringAsync();

                var obj = JsonConvert.DeserializeObject<ResponseNFC>(responseMessage);
                return obj.Data.Uri;
            }
            return string.Empty;
        }
    }
}
