using System.Runtime.Serialization;
using MyServiceBus.TcpContracts;

namespace Service.Liquidity.Engine.Grpc.Models
{
    [DataContract]
    public class GetLiquidityProviderLastOrderRequest
    {
        [DataMember(Order = 1)] public string BrokerId { get; set; }
        [DataMember(Order = 2)] public string Symbol { get; set; }
    }
}