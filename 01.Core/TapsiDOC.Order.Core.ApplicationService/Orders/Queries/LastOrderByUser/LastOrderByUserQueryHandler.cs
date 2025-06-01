using OKEService.Core.ApplicationServices.Queries;
using OKEService.Utilities;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;
using MyEntities = TapsiDOC.Order.Core.Domain.Orders.Entities;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Queries.LastOrderByUser
{
    public class LastOrderByUserQueryHandler : QueryHandler<LastOrderByUserQuery, MyEntities.Order>
    {
        private readonly IOrderQueryRepository repository;

        public LastOrderByUserQueryHandler(OKEServiceServices okeserviceApplicationContext, IOrderQueryRepository repository) : base(okeserviceApplicationContext)
        {
            this.repository = repository;
        }

        public override async Task<QueryResult<MyEntities.Order>> Handle(LastOrderByUserQuery request)
        {
            var result = await this.repository.FindLastOrderByUser(request.PhoneNumber);
            return Result(result);
        }
    }
}
