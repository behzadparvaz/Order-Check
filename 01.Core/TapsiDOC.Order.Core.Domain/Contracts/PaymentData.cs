using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TapsiDOC.Order.Core.Domain.Contracts
{
    public class PaymentData
    {
        public string VendorCode { get; set; }
        public bool IsSuccess { get; set; }
        public string TrackId { get; set; }
        public string DateTime { get; set; }
        public decimal Amount { get; set; }
    }
}
