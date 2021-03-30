using System.Runtime.Serialization;

namespace Service.Liquidity.Engine.Grpc.Models
{
    [DataContract]
    public class GetSymbolsRequest
    {
        [DataMember(Order = 1)] public string Source { get; set; }
    }
}