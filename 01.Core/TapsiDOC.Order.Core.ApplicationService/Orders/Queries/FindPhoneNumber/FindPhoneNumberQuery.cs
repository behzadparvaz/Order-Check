using OKEService.Core.ApplicationServices.Queries;
using OKEService.Core.Domain.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Queries.FindPhoneNumber
{
    public class FindPhoneNumberQuery:PageQuery<string>
    {
        public string OrderCode { get; set; }
    }
}
