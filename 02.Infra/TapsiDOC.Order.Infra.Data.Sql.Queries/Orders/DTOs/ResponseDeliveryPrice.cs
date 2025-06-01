using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TapsiDOC.Order.Infra.Data.Sql.Queries.Orders.DTOs
{
    public class ResponseDeliveryPrice
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string ResultCode { get; set; }
        public List<Data>  Data { get; set; }
    }

    public class Data
    {
        public decimal TotalDeliveryCost { get; set; }
        public string OrderCode { get; set; }
    }
}
