using System.Runtime.Serialization;
using MyJetWallet.Domain.Orders;

namespace Service.Liquidity.Engine.Domain.Models.LiquidityProvider
{
    [DataContract]
    public class LpOrder
    {
        public LpOrder()
        {
        }

        public LpOrder(string id, string symbol, string source, double price, double volume, OrderSide side)
        {
            Status = LpOrderStatus.New;
            Id = id;
            Symbol = symbol;
            Source = source;
            Price = price;
            Volume = volume;
            Side = side;
        }

        [DataMember(Order = 1)] public LpOrderStatus Status { get; set; }
        [DataMember(Order = 2)] public string Id { get; set; }
        [DataMember(Order = 3)] public string Symbol { get; set; }
        [DataMember(Order = 4)] public string Source { get; set; }
        [DataMember(Order = 5)] public double Price { get; set; }
        [DataMember(Order = 6)] public double Volume { get; set; }
        [DataMember(Order = 7)] public OrderSide Side { get; set; }
        [DataMember(Order = 8)] public string Message { get; set; } = "";
        [DataMember(Order = 9)] public string MeStatus { get; set; } = "OK";
    }
}