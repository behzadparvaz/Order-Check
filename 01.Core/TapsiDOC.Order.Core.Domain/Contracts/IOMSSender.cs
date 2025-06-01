using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TapsiDOC.Order.Core.Domain.Contracts
{
    public interface IOMSSender
    {
        Task SendOms(Orders.Entities.Order order);
    }
}
