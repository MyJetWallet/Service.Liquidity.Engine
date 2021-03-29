using System.Runtime.Serialization;
using Service.Liquidity.Engine.Domain.Models;

namespace Service.Liquidity.Engine.Grpc.Models
{
    [DataContract]
    public class HelloMessage : IHelloMessage
    {
        [DataMember(Order = 1)]
        public string Message { get; set; }
    }
}