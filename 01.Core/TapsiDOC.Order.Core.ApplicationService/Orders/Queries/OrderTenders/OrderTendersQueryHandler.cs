using OKEService.Core.ApplicationServices.Queries;
using OKEService.Core.Domain.Data;
using OKEService.Utilities;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;
using MyEntities = TapsiDOC.Order.Core.Domain.Orders.Entities;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Queries.OrderTenders
{
    public class OrderTendersQueryHandler : QueryHandler<OrderTendersQuery, PagedData<MyEntities.Order>>
    {
        private readonly IOrderQueryRepository query;

        public OrderTendersQueryHandler(OKEServiceServices okeserviceApplicationContext, IOrderQueryRepository query) : base(okeserviceApplicationContext)
        {
            this.query = query;
        }

        public override async Task<QueryResult<PagedData<MyEntities.Order>>> Handle(OrderTendersQuery request)
        {
            var result = await this.query.Tenders(request.OrderCode);
            return Result(result);
        }
    }
}
