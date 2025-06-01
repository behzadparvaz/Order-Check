using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TapsiDOC.Order.Core.Domain.Orders.QueryContract
{
    public class VendorModel
    {
        public string VendorCode { get; set; }
        public string VendorName { get; set; }
        public bool Status { get; set; }
        public string Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
