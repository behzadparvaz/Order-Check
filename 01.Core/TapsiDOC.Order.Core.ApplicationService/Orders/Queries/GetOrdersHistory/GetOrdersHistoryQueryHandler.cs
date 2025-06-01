using OKEService.Core.ApplicationServices.Queries;
using OKEService.Utilities;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;
using MyEntities = TapsiDOC.Order.Core.Domain.Orders.Entities;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Queries.GetOrdersHistory
{
    public class GetOrdersHistoryQueryHandler : QueryHandler<GetOrdersHistoryQuery, List<MyEntities.Order>>
    {
        private readonly IOrderQueryRepository repository;

        public GetOrdersHistoryQueryHandler(OKEServiceServices okeserviceApplicationContext, IOrderQueryRepository repository) : base(okeserviceApplicationContext)
        {
            this.repository = repository;
        }

        public override async Task<QueryResult<List<MyEntities.Order>>> Handle(GetOrdersHistoryQuery query)
        {
            var result = await repository.GetOrdersHistory(query.PhoneNumber , query.StatusId);
            return ResultAsync(result).Result;
        }
    }
}
