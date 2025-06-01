using OKEService.Core.ApplicationServices.Queries;
using OKEService.Utilities;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;
using MyEntities = TapsiDOC.Order.Core.Domain.Orders.Entities;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Queries.FindOrder
{
    public class FindOrderQueryHandler : QueryHandler<FindOrderQuery, MyEntities.Order>
    {
        private readonly IOrderQueryRepository query;

        public FindOrderQueryHandler(OKEServiceServices okeserviceApplicationContext , IOrderQueryRepository query) : base(okeserviceApplicationContext)
        {
            this.query = query;
        }

        public override async Task<QueryResult<MyEntities.Order>> Handle(FindOrderQuery request)
        {
            var result = await this.query.FindOrder(request.OrderCode, request.PhoneNumber);
            return Result(result);
        }
    }
}
