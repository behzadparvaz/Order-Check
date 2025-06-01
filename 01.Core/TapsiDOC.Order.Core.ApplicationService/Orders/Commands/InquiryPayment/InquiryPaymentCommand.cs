using OKEService.Core.ApplicationServices.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.InquiryPayment
{
    public class InquiryPaymentCommand : ICommand<bool>
    {
        public string TrackId { get; set; }
        public string OrderCode { get; set; }
    }
}
