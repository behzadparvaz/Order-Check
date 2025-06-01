using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TapsiDOC.Order.Infra.Data.Sql.Commands.Orders.DTOs
{
    public class VerifyPaymentRequest
    {
        public string OrderCode { get; set; }
        public string TrackId { get; set; }
    }
}
