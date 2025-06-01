using OKEService.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TapsiDOC.Order.Core.Domain.Orders.Entities
{
    public class Vendor : Entity
    {


        public string Name { get;  set; }
        public string PhoneNumber { get;  set; }
        public string Code { get;  set; }
        public string Address { get;  set; }
        public double Latitude { get;  set; }
        public double Longitude { get;  set; }

        public Vendor Create(string name, string phoneNumber, string code, string address, double lat, double lng)
        {
            return new()
            {
                Name = name,
                Code = code,
                PhoneNumber = phoneNumber,
                Address = address,
                Latitude = lat,
                Longitude = lng
            };
        }
        public Vendor Create(string vendorCode, double latitude, double longitude, string name = "")
        {
            return new()
            {
                Code = vendorCode,
                Latitude = latitude,
                Longitude = longitude,
                Name = name
            };
        }
    }
}
