using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Service.Liquidity.Engine.Domain.Models.Settings
{
    [DataContract]
    public class MarketMakerSettings
    {
        [DataMember(Order = 1)] public EngineMode Mode { get; set; }
        [DataMember(Order = 2)] public int MarketMakerRefreshIntervalMSec { get; set; }
        [DataMember(Order = 3)] public double UseExternalBalancePercentage { get; set; }
        [DataMember(Order = 4)] public int RefreshExternalBalanceIntervalMSec { get; set; }


        public MarketMakerSettings()
        {
        }

        public MarketMakerSettings(EngineMode mode)
        {
            Mode = mode;
        }
    }
}