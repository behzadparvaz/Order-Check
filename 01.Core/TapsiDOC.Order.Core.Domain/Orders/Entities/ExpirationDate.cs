using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OKEService.Core.Domain.Exceptions;
using TapsiDOC.Order.Core.Domain.Orders.SeedWork;

namespace TapsiDOC.Order.Core.Domain.Orders.Entities
{
    public class ExpirationDate : Enumeration
    {
        public static ExpirationDate ThreeToOneMonths = new ExpirationDate(0, nameof(ThreeToOneMonths).ToLowerInvariant());
        public static ExpirationDate ThreeToSixMonths = new ExpirationDate(1, nameof(ThreeToSixMonths).ToLowerInvariant());
        public static ExpirationDate MoreThanSixMonths = new ExpirationDate(2, nameof(MoreThanSixMonths).ToLowerInvariant());
        public ExpirationDate(int id, string name)
           : base(id, name) { }
        public static IEnumerable<ExpirationDate> List() =>
                    new[] { ThreeToOneMonths, ThreeToSixMonths, MoreThanSixMonths };


        public static ExpirationDate FromName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidValueObjectStateException($"Possible values for ExpirationDate: {String.Join(",", List().Select(s => s.Name))}");

            var state = List().SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

            if (state == null)
                throw new InvalidValueObjectStateException($"Possible values for ExpirationDate: {String.Join(",", List().Select(s => s.Name))}");

            return state;
        }

        public static ExpirationDate From(int? id)
        {
            if (id == null)
                return null;
            var state = List().SingleOrDefault(s => s.Id == id);
            if (state == null)
                throw new InvalidValueObjectStateException($"Possible values for ExpirationDate: {String.Join(",", List().Select(s => s.Name))}");

            return state;
        }

    }
}
