using System.Runtime.Serialization;

namespace Service.Liquidity.Engine.Domain.Models.OrderBooks
{
    [DataContract]
    public class LeOrderBookLevel
    {
        public LeOrderBookLevel()
        {
        }

        public LeOrderBookLevel(double price, double volume)
        {
            Price = price;
            Volume = volume;
        }

        [DataMember(Order = 1)] public double Price { get; set; }
        [DataMember(Order = 2)] public double Volume { get; set; }

        public static LeOrderBookLevel Create(double?[] priceVolume)
        {
            if (priceVolume.Length != 2 || !priceVolume[0].HasValue || !priceVolume[1].HasValue)
                return null;

            return new LeOrderBookLevel(priceVolume[0].Value, priceVolume[1].Value);
        }
    }
}