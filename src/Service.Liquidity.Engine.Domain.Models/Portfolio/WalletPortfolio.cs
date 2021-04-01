using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Service.Liquidity.Engine.Domain.Models.Portfolio
{
    [DataContract]
    public class WalletPortfolio
    {
        [DataMember(Order = 1)] public string WalletName { get; set; }

        [DataMember(Order = 2)] public string BrokerId { get; set; }
        [DataMember(Order = 3)] public string ClientId { get; set; }
        [DataMember(Order = 4)] public string WalletId { get; set; }

        [DataMember(Order = 5)] public List<PositionPortfolio> Positions { get; set; } = new List<PositionPortfolio>();
    }
}