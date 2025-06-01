using OKEService.Core.ApplicationServices.Commands;
using OKEService.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TapsiDOC.Order.Core.Domain.Contracts;
using TapsiDOC.Order.Core.Domain.Orders.Entities;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.RejectItemRequestOrder
{
    public class RejectItemRequestOrderHandler : CommandHandler<RejectItemRequestOrder>
    {
        private readonly IOrderCommandRepository command;
        private readonly IOrderSmsSender smsSender;
        private readonly IOMSSender oMSSender;
        private readonly IOrderQueryRepository query;

        public RejectItemRequestOrderHandler(OKEServiceServices okeserviceServices ,
            IOrderCommandRepository command,
            IOrderSmsSender smsSender,
            IOMSSender oMSSender,
            IOrderQueryRepository query) : base(okeserviceServices)
        {
            this.command = command;
            this.smsSender = smsSender;
            this.oMSSender = oMSSender;
            this.query = query;
        }

        public async override Task<CommandResult> Handle(RejectItemRequestOrder request)
        {
            var order = await this.query.GetOrder(request.OrderCode);
            var orderFirst = order.FirstOrDefault();
            orderFirst.SetAckStatus();
            orderFirst.OrderDetails.RemoveAll(a => a.Type.Id == OrderDetailType.RequestOrder.Id);
            if (orderFirst.OrderDetails.Count == 0)
            {
                orderFirst.SetReject();
                orderFirst.Comment = request.Description;
                List<string> chunks = SplitStringIntoChunks(orderFirst.Comment, 25);
                await this.command.UpdateOrderReject(orderFirst);
                await this.smsSender.SendNotification(orderFirst , chunks);
                return Ok();
            }
            foreach (var item in order)
            {
                await this.oMSSender.SendOms(item);
            }
            await this.command.UpdateOrderDetails(orderFirst);
            await this.smsSender.SendNotification(orderFirst);
            return Ok();
        }

        public static List<string> SplitStringIntoChunks(string str, int chunkSize) 
        { 
            List<string> chunks = new List<string>(); 
            for (int i = 0; i < str.Length; i += chunkSize) 
                { 
                    string chunk = str.Substring(i, Math.Min(chunkSize, str.Length - i)); 
                    chunks.Add(chunk); 
                } 
            return chunks; 
        }
    }
}
