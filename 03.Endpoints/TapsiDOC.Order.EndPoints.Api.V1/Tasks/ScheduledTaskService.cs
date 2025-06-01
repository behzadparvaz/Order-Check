//using MediatR;
//using TapsiDOC.Order.Core.ApplicationService.Orders.Commands.OrderScheduled;

//namespace TapsiDOC.Order.EndPoints.Api.V1.Tasks
//{
//    public class ScheduledTaskService : BackgroundService
//    {
//        private readonly ILogger<ScheduledTaskService> _logger;
//        private readonly IServiceProvider _serviceProvider;
//        private readonly TimeSpan _interval = TimeSpan.FromMinutes(2); 

//        public ScheduledTaskService(ILogger<ScheduledTaskService> logger, IServiceProvider serviceProvider)
//        {
//            _logger = logger;
//            _serviceProvider = serviceProvider;
//        }

//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            while (!stoppingToken.IsCancellationRequested)
//            {
//                _logger.LogDebug("ScheduledTaskService is executing a scheduled task.");

//                using (var scope = _serviceProvider.CreateScope())
//                {
//                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
//                    var command = new OrderScheduledCommand
//                    {

//                    };
//                    try
//                    {
//                        await mediator.Send(command);

//                        _logger.LogDebug("Scheduled task executed successfully.");
//                    }
//                    catch (Exception ex)
//                    {
//                        _logger.LogError("An error occurred while executing the scheduled task. Exception {@ex}", ex);
//                    }
//                }

//                await Task.Delay(_interval, stoppingToken);
//            }
//        }
//    }


//}
