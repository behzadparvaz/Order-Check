using OKEService.Core.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using TapsiDOC.Order.Core.Domain.Orders.SeedWork;

namespace TapsiDOC.Order.Core.Domain.Orders.Entities
{
    public sealed class OrderStatus : Enumeration
    {
        public static OrderStatus Draft = new OrderStatus(0, nameof(Draft).ToLowerInvariant());
        public static OrderStatus Ack = new OrderStatus(1, nameof(Ack).ToLowerInvariant());
        public static OrderStatus Auction = new OrderStatus(2, nameof(Auction).ToLowerInvariant());
        public static OrderStatus APay = new OrderStatus(3, nameof(APay).ToLowerInvariant());
        public static OrderStatus Pick = new OrderStatus(4, nameof(Pick).ToLowerInvariant());
        public static OrderStatus NFC = new OrderStatus(5, nameof(NFC).ToLowerInvariant());
        public static OrderStatus Accept = new OrderStatus(6, nameof(Accept).ToLowerInvariant());
        public static OrderStatus ADelivery = new OrderStatus(7, nameof(ADelivery).ToLowerInvariant());
        public static OrderStatus SendDelivery = new OrderStatus(8, nameof(SendDelivery).ToLowerInvariant());
        public static OrderStatus Deliverd = new OrderStatus(9, nameof(Deliverd).ToLowerInvariant());
        public static OrderStatus Return = new OrderStatus(10, nameof(Return).ToLowerInvariant());
        public static OrderStatus CancelCustomer = new OrderStatus(11, nameof(CancelCustomer).ToLowerInvariant());
        public static OrderStatus CancelVendor = new OrderStatus(12, nameof(CancelVendor).ToLowerInvariant());
        public static OrderStatus Reject = new OrderStatus(13, nameof(Reject).ToLowerInvariant());
        public static OrderStatus CancelBiker = new OrderStatus(14, nameof(CancelBiker).ToLowerInvariant());

        public OrderStatus(int id, string name)
            : base(id, name) {}

        public static IEnumerable<OrderStatus> List() =>
                    new[] { Draft, Ack , Auction , APay, Pick, NFC, Accept, ADelivery, SendDelivery, Deliverd, Return , CancelCustomer, CancelVendor, Reject , CancelBiker };

        public static OrderStatus FromName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidValueObjectStateException($"Possible values for OrderStatus: {String.Join(",", List().Select(s => s.Name))}");

            var state = List().SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

            if (state == null)
                throw new InvalidValueObjectStateException($"Possible values for OrderStatus: {String.Join(",", List().Select(s => s.Name))}");

            return state;
        }

        public static OrderStatus From(int id)
        {
            //if (id == null)
            //    return null;
            var state = List().SingleOrDefault(s => s.Id == id);
            if (state == null)
                throw new InvalidValueObjectStateException($"Possible values for OrderStatus: {String.Join(",", List().Select(s => s.Name))}");

            return state;
        }
    }
}
