using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TapsiDOC.Order.Infra.Data.Sql.Commands.Orders.DTOs
{
    public class PaymentRequest
    {
        public string OrderCode { get; set; }
        public decimal Amount { get; set; }
        public string PhoneNumber { get; set; }
        public string VendorCode { get; set; }
    }
}
