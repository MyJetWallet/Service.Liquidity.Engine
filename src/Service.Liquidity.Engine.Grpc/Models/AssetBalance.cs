using System.Runtime.Serialization;

namespace Service.Liquidity.Engine.Grpc.Models
{
    [DataContract]
    public class AssetBalanceDto
    {
        [DataMember(Order = 1)] public string Asset { get; set; }
        [DataMember(Order = 2)] public double Balance { get; set; }

        public AssetBalanceDto()
        {
        }

        public AssetBalanceDto(string asset, double balance)
        {
            Asset = asset;
            Balance = balance;
        }
    }
}