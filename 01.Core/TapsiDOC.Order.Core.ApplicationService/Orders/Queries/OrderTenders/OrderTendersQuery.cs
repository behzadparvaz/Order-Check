using OKEService.Core.ApplicationServices.Queries;
using OKEService.Core.Domain.Data;
using MyEntities = TapsiDOC.Order.Core.Domain.Orders.Entities;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Queries.OrderTenders
{
    public class OrderTendersQuery:PageQuery<PagedData<MyEntities.Order>>
    {
        public string OrderCode { get; set; }
    }
}
