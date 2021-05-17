using System.Collections.Generic;
using System.Runtime.Serialization;
using MyJetWallet.Domain;
using Service.Liquidity.Engine.Domain.Models.Wallets;

namespace Service.Liquidity.Engine.Domain.Models.Settings
{
    [DataContract]
    public class LiquidityProviderInstrumentSettings
    {
        [DataMember(Order = 1)] public string Symbol { get; set; }

        [DataMember(Order = 2)] public EngineMode Mode { get; set; }

        [DataMember(Order = 3)] public string LpWalletName { get; set; }

        [DataMember(Order = 4)] public List<LpSourceSettings> LpSources { get; set; } = new List<LpSourceSettings>();

        [DataMember(Order = 5)] public List<LpHedgeSettings> LpHedges { get; set; } = new List<LpHedgeSettings>();

        [DataMember(Order = 6)] public EngineMode ModeHedge { get; set; }

        [DataMember(Order = 7)] public string WalletId { get; set; }

        [DataMember(Order = 8)] public string BrokerId { get; set; }
    }

    [DataContract]
    public class LpSourceSettings
    {
        [DataMember(Order = 1)] public EngineMode Mode { get; set; }

        [DataMember(Order = 2)] public string ExternalMarket { get; set; }

        [DataMember(Order = 3)] public string ExternalSymbol { get; set; }

        [DataMember(Order = 4)] public double Markup { get; set; }

        [DataMember(Order = 6)] public double MaxSellSideVolume { get; set; }

        [DataMember(Order = 7)] public double MaxBuySideVolume { get; set; }
    }

    [DataContract]
    public class LpHedgeSettings
    {
        [DataMember(Order = 1)] public EngineMode Mode { get; set; }

        [DataMember(Order = 2)] public string ExternalMarket { get; set; }

        [DataMember(Order = 3)] public string ExternalSymbol { get; set; }

        [DataMember(Order = 4)] public double MinVolume { get; set; }
    }
}