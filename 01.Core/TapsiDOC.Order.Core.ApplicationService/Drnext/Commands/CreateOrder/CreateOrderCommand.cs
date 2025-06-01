using MediatR;

namespace TapsiDOC.Order.Core.ApplicationService.Drnext.Commands.CreateOrder
{
    public class CreateOrderCommand : IRequest<CreateOrderDto>
    {
        public string prescriptionTrackingCode { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string nationalCode { get; set; }
        public string mobile { get; set; }
        public int patientInsurance { get; set; }
        public List<PatientDrug> patientDrugs { get; set; }
        public string visitedAt { get; set; }
    }
    public class Drug
    {
        public string erxCode { get; set; }
        public string genericCode { get; set; }
        public string genericName { get; set; }
        public string brandName { get; set; }
        public string dosageForm { get; set; }
        public string moleculeName { get; set; }
        public string dose { get; set; }
        public string brand { get; set; }
        public string description { get; set; }
    }
    public class PatientDrug
    {
        public int amount { get; set; }
        public string direction { get; set; }
        public string description { get; set; }
        public string repeat { get; set; }
        public string dateOfDo { get; set; }
        public Drug drug { get; set; }  
        public Unit unit { get; set; }
    }
    public class Unit
    {
        public string description { get; set; }
    }
}
