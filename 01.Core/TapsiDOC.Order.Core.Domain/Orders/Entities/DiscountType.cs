using OKEService.Core.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TapsiDOC.Order.Core.Domain.Orders.SeedWork;

namespace TapsiDOC.Order.Core.Domain.Orders.Entities
{
    public class DiscountType : Enumeration
    {
        public static DiscountType Non = new DiscountType(0, nameof(Non).ToLowerInvariant());
        public static DiscountType Promotion = new DiscountType(1, nameof(Promotion).ToLowerInvariant());
        public static DiscountType Volumetric = new DiscountType(2, nameof(Volumetric).ToLowerInvariant());
        public static DiscountType Percentage = new DiscountType(3, nameof(Percentage).ToLowerInvariant());
        public static DiscountType Club = new DiscountType(4, nameof(Club).ToLowerInvariant());

        public DiscountType(int id, string name)
            : base(id, name) { }

        public static IEnumerable<DiscountType> List() =>
                    new[] {Non ,  Promotion, Volumetric, Percentage, Club};

        public static DiscountType FromName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidValueObjectStateException($"Possible values for discount type: {String.Join(",", List().Select(s => s.Name))}");

            var state = List().SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

            if (state == null)
                throw new InvalidValueObjectStateException($"Possible values for discount type: {String.Join(",", List().Select(s => s.Name))}");

            return state;
        }

        public static DiscountType From(int? id)
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
