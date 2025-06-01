using OKEService.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TapsiDOC.Order.Core.Domain.Orders.Entities
{
    public class Doctor:Entity
    {
        public string Name { get;private set; }
        public string PhoneNumber { get;private set; }
        public string Jobtitle { get;private set; }

        public Doctor Create(string name , string phoneNumber , string jobtitle)
        {
            return new()
            {
                Name = name,
                PhoneNumber = phoneNumber,
                Jobtitle = jobtitle
            };
        }
    }
}
