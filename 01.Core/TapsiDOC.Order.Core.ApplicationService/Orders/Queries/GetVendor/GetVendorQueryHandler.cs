using OKEService.Core.ApplicationServices.Queries;
using OKEService.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TapsiDOC.Order.Core.Domain.Orders.QueryContract;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Queries.GetVendor
{
    public class GetVendorQueryHandler : QueryHandler<GetVendorQuery, List<VendorType>>
    {
        private readonly IOrderQueryRepository repository;

        public GetVendorQueryHandler(OKEServiceServices okeserviceApplicationContext , IOrderQueryRepository repository) : base(okeserviceApplicationContext)
        {
            this.repository = repository;
        }

        public override async Task<QueryResult<List<VendorType>>> Handle(GetVendorQuery request)
        {
            var result = await this.repository.GetVendors();
            return Result(result);
        }
    }
}
