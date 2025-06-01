using System.Collections.Generic;
using System.Threading.Tasks;
using TapsiDOC.Order.Core.Domain.Orders.Entities;

namespace TapsiDOC.Order.Core.Domain.Contracts
{
    public interface IRequestOrder
    {
        Task SendRequestOrder(List<OrderDetail> orderDetails , string orderCode , string nationalCode , string phoneNumber , string fullName, string description , string token);
    }
}
