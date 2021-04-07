using System.Runtime.Serialization;

namespace Service.Liquidity.Engine.Domain.Models.ExternalMarkets
{
    [DataContract]
    public class ExchangeMarketInfo
    {
        [DataMember(Order = 1)] public string Market { get; set; }
        [DataMember(Order = 2)] public int PriceAccuracy { get; set; }
        [DataMember(Order = 3)] public double MinVolume { get; set; }
        [DataMember(Order = 4)] public string BaseAsset { get; set; }
        [DataMember(Order = 5)] public string QuoteAsset { get; set; }
        [DataMember(Order = 6)] public int VolumeAccuracy { get; set; }
    }
}