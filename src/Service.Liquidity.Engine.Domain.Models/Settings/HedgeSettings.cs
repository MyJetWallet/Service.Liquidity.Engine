using System.Runtime.Serialization;

namespace Service.Liquidity.Engine.Domain.Models.Settings
{
    [DataContract]
    public class HedgeSettings
    {
        [DataMember(Order = 1)] public EngineMode Mode { get; set; }

        [DataMember(Order = 2)] public int HedgeTimerIntervalMSec { get; set; }

        [DataMember(Order = 3)] public double MinVolume { get; set; }
    }
}