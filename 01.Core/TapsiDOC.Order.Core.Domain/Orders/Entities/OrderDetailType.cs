using OKEService.Core.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using TapsiDOC.Order.Core.Domain.Orders.SeedWork;

namespace TapsiDOC.Order.Core.Domain.Orders.Entities
{
    public class OrderDetailType : Enumeration
    {
        public static OrderDetailType RX = new(1, nameof(RX).ToLowerInvariant());
        public static OrderDetailType Otc = new(2, nameof(Otc).ToLowerInvariant());
        public static OrderDetailType RequestOrder = new(3, nameof(RequestOrder).ToLowerInvariant());
        public static OrderDetailType Supplement = new(4, nameof(Supplement).ToLowerInvariant());
        public OrderDetailType(int id, string name)
            : base(id, name)
        {

        }

        public static IEnumerable<OrderDetailType> List() =>
                    new[] { RX, Otc  , RequestOrder , Supplement };

        public static OrderDetailType FromName(string name)
        {
            var state = List()
                .SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

            if (state == null)
            {
                throw new InvalidValueObjectStateException($"Possible values for CollectRecieptStatus: {String.Join(",", List().Select(s => s.Name))}");
            }

            return state;
        }

        public static OrderDetailType From(int id)
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
