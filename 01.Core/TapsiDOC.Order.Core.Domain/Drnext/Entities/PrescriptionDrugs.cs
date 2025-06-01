using System;
using System.ComponentModel.DataAnnotations;

namespace TapsiDOC.Order.Core.Domain.Drnext.Entities
{
    public class PrescriptionDrugs : BaseEntity
    {
        public long PrescriptionId { get; set; }
        [MaxLength(100)]
        public string ERXCode { get; set; }
        [MaxLength(50)]
        public string GenericCode { get; set; }
        [MaxLength(350)]
        public string GenericName { get; set; }
        [MaxLength(350)]
        public string BrandName { get; set; }
        [MaxLength(150)]
        public string DosageForm { get; set; }
        [MaxLength(150)]
        public string MoleculeName { get; set; }
        [MaxLength(250)]
        public string Dose { get; set; }
        [MaxLength(250)]
        public string Brand { get; set; }
        [MaxLength(250)]
        public string Description { get; set; }

        public static PrescriptionDrugs Create(
        long prescriptionId,
        string erxCode,
        string genericCode,
        string genericName,
        string brandName,
        string dosageForm,
        string moleculeName,
        string dose,
        string brand,
        string description)
        {
            return new PrescriptionDrugs
            {
                PrescriptionId = prescriptionId,
                ERXCode = erxCode,
                GenericCode = genericCode,
                GenericName = genericName,
                BrandName = brandName,
                DosageForm = dosageForm,
                MoleculeName = moleculeName,
                Dose = dose,
                Brand = brand,
                Description = description,
                CreatedAt = DateTime.Now,

            };
        }
    }
}
