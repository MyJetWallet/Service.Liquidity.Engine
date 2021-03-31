using System.Runtime.Serialization;

namespace Service.Liquidity.Engine.Domain.Models.Settings
{
    [DataContract]
    public class MirroringLiquiditySettings
    {
        [DataMember(Order = 1)] public EngineMode Mode { get; set; }

        [DataMember(Order = 2)] public string InstrumentSymbol { get; set; }

        [DataMember(Order = 3)] public string ExternalMarket { get; set; }

        [DataMember(Order = 4)] public string ExternalSymbol { get; set; }

        [DataMember(Order = 5)] public double Markup { get; set; }

        [DataMember(Order = 6)] public string WalletName { get; set; }

        [DataMember(Order = 7)] public double MaxSellVolume { get; set; }

        [DataMember(Order = 8)] public double MaxBuyOppositeVolume { get; set; }
    }
}