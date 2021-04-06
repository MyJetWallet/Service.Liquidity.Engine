using System;
using System.Runtime.Serialization;
using MyJetWallet.Domain.Orders;

namespace Service.Liquidity.Engine.Domain.Models.ExternalMarkets
{
    [DataContract]
    public class ExchangeTrade
    {
        [DataMember(Order = 1)] public string Id { get; set; }
        [DataMember(Order = 2)] public string ReferenceId { get; set; }

        [DataMember(Order = 3)] public string Market { get; set; }
        [DataMember(Order = 4)] public OrderSide Side { get; set; }
        [DataMember(Order = 5)] public double Price { get; set; }
        [DataMember(Order = 6)] public double Volume { get; set; }
        [DataMember(Order = 7)] public double OppositeVolume { get; set; }

        [DataMember(Order = 8)] public DateTime Timestamp { get; set; }

        [DataMember(Order = 9)] public string AssociateWalletId { get; set; }
        [DataMember(Order = 10)] public string AssociateBrokerId { get; set; }
        [DataMember(Order = 11)] public string AssociateClientId { get; set; }
        [DataMember(Order = 12)] public string AssociateSymbol { get; set; }
        [DataMember(Order = 13)] public string Source { get; set; }
    }
}