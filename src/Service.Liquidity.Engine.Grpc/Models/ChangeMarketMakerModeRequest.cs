using System.Runtime.Serialization;
using Service.Liquidity.Engine.Domain.Models.Settings;

namespace Service.Liquidity.Engine.Grpc.Models
{
    [DataContract]
    public class ChangeMarketMakerModeRequest
    {
        [DataMember(Order = 1)] public EngineMode Mode { get; set; }
    }
}