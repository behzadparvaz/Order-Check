using OKEService.Core.ApplicationServices.Queries;
using OKEService.Utilities;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;
using MyEntities = TapsiDOC.Order.Core.Domain.Orders.Entities;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Queries.GetOrderDraftByOrderCode
{
    public class GetOrderDraftByOrderCodeQueryHandler : QueryHandler<GetOrderDraftByOrderCodeQuery, MyEntities.Order>
    {
        private readonly IOrderQueryRepository query;

        public GetOrderDraftByOrderCodeQueryHandler(OKEServiceServices okeserviceApplicationContext, IOrderQueryRepository query) : base(okeserviceApplicationContext)
        {
            this.query = query;
        }

        public override async Task<QueryResult<MyEntities.Order>> Handle(GetOrderDraftByOrderCodeQuery request)
        {

            var result = await this.query.GetOrderDraft(request.PhoneNumber, request.OrderCode);
            return Result(result);
        }
    }
}
