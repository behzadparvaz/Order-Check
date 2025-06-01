using OKEService.Core.ApplicationServices.Queries;
using MyEntities = TapsiDOC.Order.Core.Domain.Orders.Entities;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Queries.GetOrdersHistory
{
    public class GetOrdersHistoryQuery : PageQuery<List<MyEntities.Order>>
    {
        public string? PhoneNumber { get; set; }
        public int StatusId { get; set; } = -1;
    }
}
