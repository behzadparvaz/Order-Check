using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TapsiDOC.Order.Core.Domain.Orders.CommonContract
{
    public interface IAlternative
    {
        public string Id { get; set; }
        public string ItemNumber { get; set; }
        public List<IAlternativeItem> AlternativeItems { get; set; }
    }

    public class IAlternativeItem
    {
        public string ItemNumber { get; set; }
        public int Quantity { get; set; }
    }
}
