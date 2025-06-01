using OKEService.Core.ApplicationServices.Queries;
using OKEService.Core.Domain.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TapsiDOC.Order.Core.Domain.Orders.QueryContract;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Queries.GetVendor
{
    public class GetVendorQuery:PageQuery<List<VendorType>>
    {
    }
}
