using MyNoSqlServer.Abstractions;
using Service.Liquidity.Engine.Domain.Models.Wallets;

namespace Service.Liquidity.Engine.Domain.Models.NoSql
{
    public class LpWalletNoSql: MyNoSqlDbEntity
    {
        public const string TableName = "myjetwallet-liquitityprovider-wallets";

        public static string GeneratePartitionKey() => "wallets";
        public static string GenerateRowKey(string walletName) => walletName;

        public LpWallet Wallet { get; set; }

        public static LpWalletNoSql Create(LpWallet wallet)
        {
            return new LpWalletNoSql()
            {
                PartitionKey = LpWalletNoSql.GeneratePartitionKey(),
                RowKey = LpWalletNoSql.GenerateRowKey(wallet.Name),
                Wallet = wallet
            };
        }
    }
}