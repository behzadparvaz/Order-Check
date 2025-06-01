using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TapsiDOC.Order.Core.Domain.Orders.CommonContract;

namespace TapsiDOC.Order.Core.Domain.Orders.Repositories
{
    public interface IOutboxQueryRepository
    {
        Task<List<OutBoxEvent>> GetOutBoxItemsByAggregate(string aggregate);
    }
}
