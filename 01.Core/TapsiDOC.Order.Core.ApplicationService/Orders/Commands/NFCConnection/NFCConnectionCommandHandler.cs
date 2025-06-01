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

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.NFCConnection
{
    public class NFCConnectionCommandHandler : CommandHandler<NFCConnectionCommand>
    {
        private readonly IOrderQueryRepository query;
        private readonly IOrderSmsSender smsSender;
        private readonly INFCConnection connection;

        public NFCConnectionCommandHandler(OKEServiceServices okeserviceServices, IOrderQueryRepository query,
                        IOrderSmsSender smsSender, INFCConnection connection) : base(okeserviceServices)
        {
            this.query = query;
            this.smsSender = smsSender;
            this.connection = connection;
        }

        public override async Task<CommandResult> Handle(NFCConnectionCommand request)
        {
            var order = await this.query.FindOrderByVendor(request.OrderCode, request.VendorCode);
            order.OrderStatus = OrderStatus.NFC;
            var uri = await this.connection.JoinConnection(request.MeetId ,string.IsNullOrEmpty(order.Customer.Name)?order.Customer.PhoneNumber:order.Customer.Name);
            int index = uri.IndexOf("sessionToken=") + "sessionToken=".Length;
            string token = uri.Substring(index);
            await this.smsSender.SendNotification(order, null, token);
            return Ok();
        }
    }
}
