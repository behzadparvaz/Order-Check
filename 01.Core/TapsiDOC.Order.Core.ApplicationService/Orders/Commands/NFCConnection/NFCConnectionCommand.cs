using OKEService.Core.ApplicationServices.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.NFCConnection
{
    public class NFCConnectionCommand:ICommand
    {
        public string OrderCode { get; set; }
        public string VendorCode { get; set; }
        public string VendorName { get; set; }
        public string MeetId { get; set; }
    }
}
