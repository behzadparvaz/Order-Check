using OKEService.Core.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using TapsiDOC.Order.Core.Domain.Orders.SeedWork;

namespace TapsiDOC.Order.Core.Domain.Orders.Entities
{
    public class InsuranceType : Enumeration
    {
        public static InsuranceType Non = new InsuranceType(0, nameof(Non).ToLowerInvariant());
        public static InsuranceType Tamin = new InsuranceType(1, "بیمه تامین اجتماعی");
        public static InsuranceType Salamat = new InsuranceType(2, "بیمه سلامت ایران");
        public static InsuranceType Niruo = new InsuranceType(3, "بیمه نیروهای مسلح");

        public InsuranceType(int id, string name)
            : base(id, name) { }

        public static IEnumerable<InsuranceType> List() =>
                    new[] { Non ,  Tamin, Salamat, Niruo };

        public static InsuranceType FromName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidValueObjectStateException($"Possible values for discount type: {String.Join(",", List().Select(s => s.Name))}");

            var state = List().SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

            if (state == null)
                throw new InvalidValueObjectStateException($"Possible values for discount type: {String.Join(",", List().Select(s => s.Name))}");

            return state;
        }

        public static InsuranceType From(int? id)
        {
            if (id == null)
                return null;
            var state = List().SingleOrDefault(s => s.Id == id);
            if (state == null)
                throw new InvalidValueObjectStateException($"Possible values for discount type: {String.Join(",", List().Select(s => s.Name))}");

            return state;
        }
    }
}
