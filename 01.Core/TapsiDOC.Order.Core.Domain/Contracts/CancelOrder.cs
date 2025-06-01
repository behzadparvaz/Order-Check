using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TapsiDOC.Order.Core.Domain.Contracts
{
    public class CancelOrder
    {
        public string OrderCode { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public int DeclineId { get; set; }
        public string DeclineName { get; set; }
    }
}
