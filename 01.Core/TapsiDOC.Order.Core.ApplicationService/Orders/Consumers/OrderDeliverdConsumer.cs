using BuildingBlock.Messaging.Command;
using Dapper;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OKEService.Core.ApplicationServices.Commands;
using System.Data.SqlClient;
using TapsiDOC.Order.Core.ApplicationService.Orders.Commands.UpdateOrderStatus;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Consumers
{
    public class OrderDeliverdConsumer : IConsumer<OrderDeliveryCommand>
    {
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly ILogger<OrderDeliverdConsumer> _logger;
        private readonly IConfiguration _configuration;

        public OrderDeliverdConsumer(ICommandDispatcher commandDispatcher, ILogger<OrderDeliverdConsumer> logger, IConfiguration configuration)
        {
            _commandDispatcher = commandDispatcher;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task Consume(ConsumeContext<OrderDeliveryCommand> context)
        {
            var reqId = Guid.NewGuid().ToString("N");
            var connectionString = _configuration.GetConnectionString("loggerConnectionString")
                    ?? throw new Exception("LOGGER CONNECTION STRING NOT FOUND");

            using var _connection = new SqlConnection(connectionString);
            var message = context.Message;

            try
            {

                _ = Task.Run(() =>
                {
                    _connection.ExecuteAsync($"EXEC PRC_LOGGER_RABBIT " +
                    $"@AggregateName = N'Order.Core.ApplicationService.Orders.Consumers.OrderDeliverdConsumer'," +
                    $"@OrderCode = N'{message.OrderCode}'," +
                    $"@VendorCode = N'{message.VendorCode}'," +
                    $"@Message= N'{JsonConvert.SerializeObject(context.Message)}'," +
                    $"@MessageId= N'{context.MessageId}', " +
                    $"@RequestId= N'{reqId}'," +
                    $"@Description= 'REQUEST'");
                });

                var result = await _commandDispatcher.Send(new UpdateOrderStatusCommand()
                {
                    AggregateIdentifier = message.AggregateIdentifier,
                    AggregateType = message.AggregateType,
                    CreateDateTimeEvent = message.CreateDateTimeEvent,
                    DateTimeRaiseEvent = message.DateTimeRaiseEvent,
                    EventType = message.EventType,
                    Id = message.Id,
                    Version = message.Version,
                    VendorCode = message.VendorCode,
                    OrderCode = message.OrderCode,
                    StatusId = message.StatusId,
                });
                _ = Task.Run(() =>
                {
                    _connection.ExecuteAsync($"EXEC PRC_LOGGER_RABBIT " +
                    $"@AggregateName = N'Order.Core.ApplicationService.Orders.Consumers.OrderDeliverdConsumer'," +
                    $"@OrderCode = N'{message.OrderCode}'," +
                    $"@VendorCode = N'{message.VendorCode}'," +
                    $"@Message= N'{JsonConvert.SerializeObject(result)}'," +
                    $"@MessageId= N'{context.MessageId}', " +
                    $"@RequestId= N'{reqId}'," +
                    $"@Description= 'RESPONSE'");
                });
                if (result.Status != OKEService.Core.ApplicationServices.Common.ApplicationServiceStatus.Ok)
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                _ = Task.Run(() =>
                {
                    _connection.ExecuteAsync($"EXEC PRC_LOGGER_RABBIT " +
                    $"@AggregateName = N'Order.Core.ApplicationService.Orders.Consumers.OrderDeliverdConsumer'," +
                    $"@OrderCode = N'{message.OrderCode}'," +
                    $"@VendorCode = N'{message.VendorCode}'," +
                    $"@Message= N'{JsonConvert.SerializeObject(context.Message)}'," +
                    $"@MessageId= N'{context.MessageId}', " +
                    $"@RequestId= N'{reqId}'," +
                    $"@Description= 'EXCEPTION, {ex.Message}'");
                });
                throw;
            }
        }
    }
}
