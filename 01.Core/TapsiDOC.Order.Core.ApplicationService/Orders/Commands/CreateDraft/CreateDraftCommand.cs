using OKEService.Core.ApplicationServices.Commands;
using System.Reflection.PortableExecutable;
using TapsiDOC.Order.Core.Domain.Orders.CommonContract;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.CreateDraft
{
    public class CreateDraftCommand : ICommand<string>
    {
        public string ReferenceNumber { get; set; }
        public int InsuranceTypeId { get; set; }
        public int SupplementaryInsuranceTypeId { get; set; }
        public string? DoctorName { get; set; }
        public string? Comment { get; set; }
        public string? PhoneNumber { get; set; }
        public string NationalCode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? CustomerName { get; set; }
        public string? ValueAddress { get; set; }
        public string? TitleAddress { get; set; }
        public string? HouseNumber { get; set; }
        public int HomeUnit { get; set; }
        public bool IsSpecialPatient { get; set; } = false;
        public string? DeliveryDate { get; set; }
        public List<VendorSelect> VendorSelects { get ; set ; }
    }

    public class Item
    {
        public string IRC { get; set; }
        public string GTIN { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
    }

}
