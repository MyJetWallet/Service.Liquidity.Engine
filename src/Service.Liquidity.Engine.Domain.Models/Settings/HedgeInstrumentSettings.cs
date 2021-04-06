using System.Runtime.Serialization;

namespace Service.Liquidity.Engine.Domain.Models.Settings
{
    [DataContract]
    public class HedgeInstrumentSettings
    {
        [DataMember(Order = 1)] public EngineMode Mode { get; set; }

        [DataMember(Order = 2)] public string InstrumentSymbol { get; set; }

        [DataMember(Order = 3)] public string ExternalMarket { get; set; }

        [DataMember(Order = 4)] public string ExternalSymbol { get; set; }

        [DataMember(Order = 5)] public double MinVolume { get; set; }

        [DataMember(Order = 6)] public string WalletId { get; set; }
    }
}