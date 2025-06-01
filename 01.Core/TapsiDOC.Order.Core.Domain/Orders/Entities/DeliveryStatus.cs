using OKEService.Core.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TapsiDOC.Order.Core.Domain.Orders.SeedWork;

namespace TapsiDOC.Order.Core.Domain.Orders.Entities
{
    public class DeliveryStatus : Enumeration
    {
        public static DeliveryStatus Draft = new DeliveryStatus(0, nameof(Draft).ToLowerInvariant());
        public static DeliveryStatus Pickup = new DeliveryStatus(1, nameof(Pickup).ToLowerInvariant());
        public static DeliveryStatus Delivered = new DeliveryStatus(2, nameof(Delivered).ToLowerInvariant());
        public static DeliveryStatus CancelBiker = new DeliveryStatus(3, nameof(CancelBiker).ToLowerInvariant());

        public DeliveryStatus(int id, string name)
            : base(id, name) { }
        public static IEnumerable<DeliveryStatus> List() =>
                    new[] { Draft, Pickup, Delivered };
        public static DeliveryStatus FromName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidValueObjectStateException($"Possible values for DeliveryStatus: {String.Join(",", List().Select(s => s.Name))}");

            var state = List().SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

            if (state == null)
                throw new InvalidValueObjectStateException($"Possible values for DeliveryStatus: {String.Join(",", List().Select(s => s.Name))}");

            return state;
        }
        public static DeliveryStatus From(int? id)
        {
            if (id == null)
                return null;
            var state = List().SingleOrDefault(s => s.Id == id);
            if (state == null)
                throw new InvalidValueObjectStateException($"Possible values for DeliveryStatus: {String.Join(",", List().Select(s => s.Name))}");

            return state;
        }
    }
}
