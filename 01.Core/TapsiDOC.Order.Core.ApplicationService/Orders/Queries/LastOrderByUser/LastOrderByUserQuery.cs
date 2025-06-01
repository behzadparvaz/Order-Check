using OKEService.Core.ApplicationServices.Queries;
using MyEntities = TapsiDOC.Order.Core.Domain.Orders.Entities;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Queries.LastOrderByUser
{
    public class LastOrderByUserQuery:PageQuery<MyEntities.Order>
    {
        public string? PhoneNumber { get; set; }
    }
}
