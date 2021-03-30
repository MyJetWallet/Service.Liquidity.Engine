using System.Runtime.Serialization;

namespace Service.Liquidity.Engine.Grpc.Models
{
    [DataContract]
    public class GetOrderBookRequest
    {
        [DataMember(Order = 1)] public string Symbol { get; set; }
        [DataMember(Order = 2)] public string Source { get; set; }
    }
}