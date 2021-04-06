using System.Runtime.Serialization;

namespace Service.Liquidity.Engine.Domain.Models.Settings
{
    [DataContract]
    public class HedgeSettings
    {
        [DataMember(Order = 1)] public EngineMode Mode { get; set; }
    }
}