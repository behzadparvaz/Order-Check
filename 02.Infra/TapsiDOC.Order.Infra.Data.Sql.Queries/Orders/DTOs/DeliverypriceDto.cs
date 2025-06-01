using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TapsiDOC.Order.Infra.Data.Sql.Queries.Orders.DTOs
{
    public class DeliverypriceDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; }
        public string VendorCode { get; set; }
        public int ParcelValue { get; set; }
    }
}
