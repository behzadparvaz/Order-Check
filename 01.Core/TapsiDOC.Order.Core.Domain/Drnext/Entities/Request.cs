using System;
using System.ComponentModel.DataAnnotations;

namespace TapsiDOC.Order.Core.Domain.Drnext.Entities
{
    public class Request : BaseEntity
    {
        [MaxLength(250)]
        public string FirstName { get; set; }
        [MaxLength(250)]
        public string LastName { get; set; }
        [MaxLength(50)]
        public string NationalCode { get; set; }
        [MaxLength(50)]
        public string Mobile { get; set; }
        [MaxLength(50)]
        public string PatientInsurance { get; set; }
        [MaxLength(50)]
        public string VisitedAt { get; set; }

        public static Request Create(string firstName, string lastName, string nationalCode, string mobile, string patientInsurance, string visitedAt)
        {
            return new Request
            {
                FirstName = firstName,
                LastName = lastName,
                NationalCode = nationalCode,
                Mobile = mobile,
                PatientInsurance = patientInsurance,
                VisitedAt = visitedAt,
                CreatedAt=DateTime.Now,
            };
        }
    }
}

