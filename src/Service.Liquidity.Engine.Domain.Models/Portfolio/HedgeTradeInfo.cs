using System;
using System.Runtime.Serialization;

namespace Service.Liquidity.Engine.Domain.Models.Portfolio
{
    [DataContract]
    public class HedgeTradeInfo
    {
        [DataMember(Order = 1)] public string Exchange { get; set; }
        [DataMember(Order = 2)] public string Symbol { get; set; }
        [DataMember(Order = 3)] public DateTime Timestamp { get; set; }
        [DataMember(Order = 4)] public double Price { get; set; }
        [DataMember(Order = 5)] public string TradeId { get; set; }
    }
}