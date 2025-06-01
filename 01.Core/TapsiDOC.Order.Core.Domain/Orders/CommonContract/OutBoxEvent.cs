namespace TapsiDOC.Order.Core.Domain.Orders.CommonContract
{
    public class OutBoxEvent
    {
        public long RecId { get; set; }
        public string Aggregate { get; set; }
        public string Json { get; set; }
        public string Token { get; set; }
        public bool IsProcessed { get; set; }
        public string RaiseDateTime { get; set; }
        public int StatusCode { get; set; }
        public string OrderCode { get; set; }
        public string EventName { get; set; }
        public string Step { get; set; }
        public int Version { get; set; } 
        public string ParentTraceId { get; set; }
    }
}
