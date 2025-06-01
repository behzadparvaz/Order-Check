using OKEService.Core.ApplicationServices.Queries;
using MyEntities = TapsiDOC.Order.Core.Domain.Orders.Entities;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Queries.FindOrder
{
    public class FindOrderQuery:PageQuery<MyEntities.Order>
    {
        public string OrderCode { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
