using Dapper;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OKEService.Core.ApplicationServices.Commands;
using System.Data.SqlClient;
using static MassTransit.ValidationResultExtensions;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Consumers
{
    public class AcceptOrderCosumer : IConsumer<BuildingBlock.Messaging.Command.AcceptOrderCommand>
    {
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly ILogger<AcceptOrderCosumer> _logger;
        private readonly IConfiguration _configuration;

        public AcceptOrderCosumer(ICommandDispatcher commandDispatcher, ILogger<AcceptOrderCosumer> logger, IConfiguration configuration)
        {
            _commandDispatcher = commandDispatcher;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task Consume(ConsumeContext<BuildingBlock.Messaging.Command.AcceptOrderCommand> context)
        {
            var reqId = Guid.NewGuid().ToString("N");
            var connectionString = _configuration.GetConnectionString("loggerConnectionString")
                ?? throw new Exception("LOGGER CONNECTION STRING NOT FOUND");

            using var _connection = new SqlConnection(connectionString);

            try
            {
                await _connection.ExecuteAsync($"EXEC PRC_LOGGER_RABBIT " +
                    $"@AggregateName = N'Order.Core.ApplicationService.Orders.Consumers.AcceptOrderCosumer'," +
                    $"@OrderCode = NULL," +
                    $"@VendorCode = NULL," +
                    $"@Message= N'{JsonConvert.SerializeObject(context.Message)}'," +
                    $"@MessageId= N'{context.MessageId}', " +
                    $"@RequestId= N'{reqId}_{context.Message.AggregateIdentifier}'," +
                    $"@Description= 'REQUEST'");

                var message = context.Message;

                var result = await _commandDispatcher.Send(new Commands.AcceptOrder.AcceptOrderCommand()
                {
                    Version = message.Version,
                    Id = message.Id,
                    EventType = message.EventType,
                    Event = message.Event,
                    AggregateIdentifier = message.AggregateIdentifier,
                    AggregateType = message.AggregateType,
                    CreateDateTimeEvent = message.CreateDateTimeEvent,
                    DateTimeRaiseEvent = message.DateTimeRaiseEvent,
                });
                await _connection.ExecuteAsync($"EXEC PRC_LOGGER_RABBIT " +
                    $"@AggregateName = N'Order.Core.ApplicationService.Orders.Consumers.AcceptOrderCosumer'," +
                    $"@OrderCode = NULL," +
                    $"@VendorCode = NULL," +
                    $"@Message= N'{JsonConvert.SerializeObject(result)}'," +
                    $"@MessageId= N'{context.MessageId}', " +
                    $"@RequestId= N'{reqId}_{context.Message.AggregateIdentifier}'," +
                    $"@Description= N'RESPONSE'");
            }
            catch (Exception ex)
            {
                await _connection.ExecuteAsync($"EXEC PRC_LOGGER_RABBIT " +
                  $"@AggregateName = N'Order.Core.ApplicationService.Orders.Consumers.AcceptOrderCosumer'," +
                  $"@OrderCode = NULL," +
                  $"@VendorCode = NULL," +
                  $"@Message= N'{JsonConvert.SerializeObject(context.Message)}'," +
                  $"@MessageId= N'{context.MessageId}', " +
                  $"@RequestId= N'{reqId}_{context.Message.AggregateIdentifier}'," +
                  $"@Description= 'EXCEPTION, {ex.Message}'");
                throw;
            }

        }
    }
}
