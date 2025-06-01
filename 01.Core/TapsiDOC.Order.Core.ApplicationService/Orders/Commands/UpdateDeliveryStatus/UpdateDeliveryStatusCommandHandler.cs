using OKEService.Core.ApplicationServices.Commands;
using OKEService.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.UpdateDeliveryStatus
{
    public class UpdateDeliveryStatusCommandHandler : CommandHandler<UpdateDeliveryStatusCommand>
    {
        public UpdateDeliveryStatusCommandHandler(OKEServiceServices okeserviceServices) : base(okeserviceServices)
        {
        }

        public override Task<CommandResult> Handle(UpdateDeliveryStatusCommand request)
        {
            throw new NotImplementedException();
        }
    }
}
