using OKEService.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TapsiDOC.Order.Core.Domain.Orders.Entities
{
    public class Customer : Entity
    {
        public string PhoneNumber { get; set; }
        public string NationalCode { get; set; }
        public string Name { get; set; }
        public List<Address> Addresses { get; set; }
        public string? AlternateRecipientName { get; set; }
        public string? AlternateRecipientMobileNumber { get; set; }
        public Customer()
        {
            Addresses = new List<Address>();
        }

        public static Customer Create(string phoneNumber, string nationaCode, double lat, double lng,
            string valueAddress, string titleAddress, string HouseNumber,
            int homeUnit = 0, string name = "",
            string? alternateRecipientName = null,
            string? alternateRecipientMobileNumber= null)
        {
            var address = Address.Create(lat, lng, valueAddress, titleAddress, HouseNumber, homeUnit);

            return new()
            {
                PhoneNumber = phoneNumber,
                NationalCode = nationaCode,
                Name = name,
                Addresses = new List<Address> { address },
                AlternateRecipientName = alternateRecipientName,
                AlternateRecipientMobileNumber = alternateRecipientMobileNumber
            };
        }
    }
}
