using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TapsiDOC.Order.Infra.Data.Sql.Commands.Orders.DTOs
{
    public class DeliveryModel
    {
        public string OrderCode { get; set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; }
        public string HouseNumber { get; set; }
        public int HomeUnit { get; set; }
        public string VendorName { get; set; }
        public string VendorCode { get; set; }
        public double VendorLatitude { get; set; }
        public double VendorLongitude { get; set; }
        public string VendorPhoneNumber { get; set; }
        public string VendorAddress { get; set; }
        public string DeliveryDate { get; set; }
    }
}
