using OKEService.Core.ApplicationServices.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.UpdateDeliveryPriceRoyal
{
    public class UpdateDeliveryPriceRoyalCommand:ICommand
    {
        public string OrderCode { get; set; }
        public bool IsSchedule { get; set; }
    }
}
