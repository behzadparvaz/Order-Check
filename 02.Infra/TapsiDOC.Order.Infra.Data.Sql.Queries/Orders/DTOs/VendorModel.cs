namespace TapsiDOC.Order.Infra.Data.Sql.Queries.Orders.DTOs
{
    public class VendorModel
    {
        public List<vendor>  Vendors { get; set; }
        public bool IsPreOrder { get; set; }
        public string SendOrderTime { get; set; }
    }

    public class vendor
    {
        public string VendorCode { get; set; }
        public bool IsAllTime { get; set; }
    }
}
