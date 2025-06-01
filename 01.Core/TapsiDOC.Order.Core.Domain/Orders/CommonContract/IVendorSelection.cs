using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TapsiDOC.Order.Core.Domain.Orders.CommonContract
{
    public interface IVendorSelection
    {
        public List<VendorSelect> vendorSelects { get; set; }
    }

    public class VendorSelect
    {
        public string VendorCode { get; set; }
        public string VendorName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
