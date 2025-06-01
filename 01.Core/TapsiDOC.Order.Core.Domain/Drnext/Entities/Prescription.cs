using System;
using System.ComponentModel.DataAnnotations;

namespace TapsiDOC.Order.Core.Domain.Drnext.Entities
{
    public class Prescription : BaseEntity
    {
        public int? Amount { get; set; }
        [MaxLength(500)]
        public string Direction { get; set; }
        [MaxLength(500)]
        public string Description { get; set; }
        [MaxLength(250)]
        public string Repeat { get; set; }
        [MaxLength(250)]
        public string DateOfDo { get; set; }
        [MaxLength(500)]
        public string Unit { get; set; }

        public static Prescription Create(int? amount, string direction, string description, string repeat, string dateOfDo, string unit)
        {
            return new Prescription
            {
                Amount = amount,
                Direction = direction,
                Description = description,
                Repeat = repeat,
                DateOfDo = dateOfDo,
                Unit = unit,
                CreatedAt=DateTime.Now,
            };
        }
    }
}
