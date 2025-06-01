using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TapsiDOC.Order.Infra.Data.Sql.Commands.Orders.DTOs
{
    public class CreateOrderInsuranceOMS
    {
        public string OrderCode { get; set; }
        public int InsuranceTypeId { get; set; }
        public int SupplementaryInsuranceTypeId { get; set; }
        public string comment { get; set; }
        public string ReferenceNumber { get; set; }
        public string CustomerName { get; set; }
        public string NationalCode { get; set; }
        public string AddressValue { get; set; }
        public bool IsSpecialPatient { get; set; }
        public string CreateDateTime { get; set; }
        public string VendorCode { get; set; }
        public string PhoneNumberCustomer { get; set; }
    }
}
