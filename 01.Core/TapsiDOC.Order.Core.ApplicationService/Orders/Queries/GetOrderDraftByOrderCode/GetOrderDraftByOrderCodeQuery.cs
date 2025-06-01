using OKEService.Core.ApplicationServices.Queries;
using MyEntities = TapsiDOC.Order.Core.Domain.Orders.Entities;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Queries.GetOrderDraftByOrderCode
{
    public class GetOrderDraftByOrderCodeQuery: PageQuery<MyEntities.Order>
    {
        public string? PhoneNumber { get; set; }
        public string OrderCode { get; set; }
    }
}
