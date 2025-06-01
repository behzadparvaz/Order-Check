using System.Collections.Generic;
using System.Threading.Tasks;

namespace TapsiDOC.Order.Core.Domain.Contracts
{
    public interface IOrderSmsSender
    {
        Task SendNotification(Orders.Entities.Order order, List<string> chunks = null, string uriConnection = "");
        Task SendNotification(Core.Domain.Orders.Entities.Order order);
        Task SendNotificationScheduledOrder(Core.Domain.Orders.Entities.Order order);
    }
}
