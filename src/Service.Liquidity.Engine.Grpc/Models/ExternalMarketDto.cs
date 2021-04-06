using System.Runtime.Serialization;

namespace Service.Liquidity.Engine.Grpc.Models
{
    [DataContract]
    public class SourceDto
    {
        [DataMember(Order = 1)] public string Source { get; set; }
    }
}