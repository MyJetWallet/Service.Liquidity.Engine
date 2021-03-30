using System.Runtime.Serialization;

namespace Service.Liquidity.Engine.Grpc.Models
{
    [DataContract]
    public class GetSourcesWithSymbolRequest
    {
        [DataMember(Order = 1)] public string Symbol { get; set; }
    }
}