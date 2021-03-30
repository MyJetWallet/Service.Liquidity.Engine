using System.Runtime.Serialization;

namespace Service.Liquidity.Engine.Grpc.Models
{
    [DataContract]
    public class WalletNameRequest
    {
        [DataMember(Order = 1)] public string WalletName { get; set; }
    }
}