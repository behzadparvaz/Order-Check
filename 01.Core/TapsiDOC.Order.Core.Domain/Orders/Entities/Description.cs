using OKEService.Core.Domain.Entities;

namespace TapsiDOC.Order.Core.Domain.Orders.Entities
{
    public class Description:Entity
    {
        public string Comment { get;  set; }
        public string Link { get;  set; }

        public static Description Create(string comment, string link)
        {
            return new()
            {
                Comment = comment,
                Link = link
            };
        }

        public void SetDescription(string comment, string link)
        {
            this.Link = link;
            this.Comment = comment;
        }
    }
}
