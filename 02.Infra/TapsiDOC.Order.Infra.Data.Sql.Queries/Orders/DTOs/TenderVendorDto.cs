using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TapsiDOC.Order.Infra.Data.Sql.Queries.Orders.DTOs
{
    public class TenderVendorDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Radis { get; set; }
    }
}
