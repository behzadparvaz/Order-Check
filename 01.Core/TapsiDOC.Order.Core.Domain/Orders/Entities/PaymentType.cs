using OKEService.Core.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TapsiDOC.Order.Core.Domain.Orders.SeedWork;

namespace TapsiDOC.Order.Core.Domain.Orders.Entities
{
    public class PaymentType : Enumeration
    {
        public static PaymentType Cash = new PaymentType(100, nameof(Cash).ToLowerInvariant());
        public static PaymentType Online = new PaymentType(200, nameof(Online).ToLowerInvariant());
        public static PaymentType Card = new PaymentType(300, nameof(Card).ToLowerInvariant());
        public static PaymentType CashOnline = new PaymentType(400, nameof(CashOnline).ToLowerInvariant());
        public static PaymentType Cardit = new PaymentType(500, nameof(Cardit).ToLowerInvariant());
        public PaymentType(int id, string name)
            : base(id, name)
        {

        }

        public static IEnumerable<PaymentType> List() =>
                    new[] { Cash, Online, Card, CashOnline , Cardit };

        public static PaymentType FromName(string name)
        {
            var state = List()
                .SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

            if (state == null)
            {
                throw new InvalidValueObjectStateException($"Possible values for CollectRecieptStatus: {String.Join(",", List().Select(s => s.Name))}");
            }

            return state;
        }

        public static PaymentType From(int id)
        {
            var state = List().SingleOrDefault(s => s.Id == id);

            if (state == null)
            {
                throw new InvalidValueObjectStateException($"Possible values for CollectRecieptStatus: {String.Join(",", List().Select(s => s.Name))}");
            }

            return state;
        }
    }
}
