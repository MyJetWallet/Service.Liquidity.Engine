using System.Runtime.Serialization;

namespace Service.Liquidity.Engine.Domain.Models.Wallets
{
    [DataContract]
    public class LpWallet
    {
        [DataMember(Order = 1)] public string Name { get; set; }

        [DataMember(Order = 2)] public string BrokerId { get; set; }

        [DataMember(Order = 3)] public string ClientId { get; set; }

        [DataMember(Order = 4)] public string WalletId { get; set; }
    }
}