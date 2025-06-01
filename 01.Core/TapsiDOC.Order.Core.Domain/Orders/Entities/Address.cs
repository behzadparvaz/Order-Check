using OKEService.Core.Domain.Entities;

namespace TapsiDOC.Order.Core.Domain.Orders.Entities
{
    public class Address:Entity
    {
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public string TitleAddress { get;private set; }
        public string ValueAddress { get;private set; }
        public string HouseNumber { get;private set; }
        public int HomeUnit { get;private set; }
        
        public Address() { }
        
        public static Address Create(double latitude , double longitude , string valueAddress ,  string titleAddress = "" , string houseNumber = "" , int homeUnit = 0)
        {
            return new()
            {
                Latitude = latitude,
                Longitude = longitude,
                ValueAddress = valueAddress,
                TitleAddress = titleAddress,
                HouseNumber = houseNumber,
                HomeUnit = homeUnit

            };
        }
    }
}
