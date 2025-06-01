namespace TapsiDOC.Order.Infra.Data.Sql.Commands.Orders.DTOs
{
    public class CreateTenderDto
    {
        public string OrderCode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
