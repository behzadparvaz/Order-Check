using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TapsiDOC.Order.Core.Domain.Orders.Entities;

namespace TapsiDOC.Order.Infra.Data.Sql.Queries.Orders.DTOs
{
    public class OrderDto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string? OrderCode { get; private set; }
        public Customer? Customer { get; private set; }
        public string? PharmacyCode { get; private set; }
        public OrderStatus? OrderStatus { get; private set; }
        public string? CreateDateTime { get; set; }
        public string? ModifiedDateTime { get; set; }
        public string? CreateBy { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
