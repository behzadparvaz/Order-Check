using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TapsiDOC.Order.Infra.Data.Sql.Commands.Orders.DTOs
{
   public class VendorModel
    {
        public string VendorCode { get; set; }

        public string VendorName { get; set; }
        public Location  Location { get; set; }
    }

    public class Location
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
