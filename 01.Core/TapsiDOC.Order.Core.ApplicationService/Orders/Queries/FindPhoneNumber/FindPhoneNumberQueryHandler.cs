using OKEService.Core.ApplicationServices.Queries;
using OKEService.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Queries.FindPhoneNumber
{
    public class FindPhoneNumberQueryHandler : QueryHandler<FindPhoneNumberQuery, string>
    {
        private readonly IOrderQueryRepository query;

        public FindPhoneNumberQueryHandler(OKEServiceServices okeserviceApplicationContext,
                                            IOrderQueryRepository query) : base(okeserviceApplicationContext)
        {
            this.query = query;
        }

        public override async Task<QueryResult<string>> Handle(FindPhoneNumberQuery request)
        {
            var phone = await this.query.FindPhoneNumber(request.OrderCode);
            return Result(phone);
        }
    }
}
