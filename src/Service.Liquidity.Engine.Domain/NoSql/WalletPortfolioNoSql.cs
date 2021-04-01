using MyNoSqlServer.Abstractions;
using Service.Liquidity.Engine.Domain.Models.Portfolio;

namespace Service.Liquidity.Engine.Domain.NoSql
{
    public class WalletPortfolioNoSql: MyNoSqlDbEntity
    {
        public const string TableName = "myjetwallet-liquitityprovider-portfolio";

        public static string GeneratePartitionKey(string walletId) => $"walletId={walletId}";
        public static string GenerateRowKey() => "portfolio";

        public WalletPortfolio Portfolio { get; set; }

        public static WalletPortfolioNoSql Create(WalletPortfolio portfolio)
        {
            return new WalletPortfolioNoSql()
            {
                PartitionKey = GeneratePartitionKey(portfolio.WalletId),
                RowKey = GenerateRowKey(),
                Portfolio = portfolio
            };
        }
    }
}