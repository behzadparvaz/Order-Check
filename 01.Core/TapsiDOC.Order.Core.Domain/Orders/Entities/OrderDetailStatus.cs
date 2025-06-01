using OKEService.Core.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using TapsiDOC.Order.Core.Domain.Orders.SeedWork;

namespace TapsiDOC.Order.Core.Domain.Orders.Entities
{
    public class OrderDetailStatus : Enumeration
    {
        public static OrderDetailStatus Draft = new OrderDetailStatus(1, nameof(Draft).ToLowerInvariant());
        public static OrderDetailStatus Collect = new OrderDetailStatus(2, nameof(Collect).ToLowerInvariant());
        public static OrderDetailStatus Pick = new OrderDetailStatus(3, nameof(Pick).ToLowerInvariant());
        public static OrderDetailStatus NFC = new OrderDetailStatus(4, nameof(NFC).ToLowerInvariant());
        public OrderDetailStatus(int id, string name)
            : base(id, name)
        {

        }

        public static IEnumerable<OrderDetailStatus> List() =>
                    new[] { Draft , Collect, Pick, NFC};

        public static OrderDetailStatus FromName(string name)
        {
            var state = List()
                .SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

            if (state == null)
            {
                throw new InvalidValueObjectStateException($"Possible values for CollectRecieptStatus: {String.Join(",", List().Select(s => s.Name))}");
            }

            return state;
        }

        public static OrderDetailStatus From(int id)
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
