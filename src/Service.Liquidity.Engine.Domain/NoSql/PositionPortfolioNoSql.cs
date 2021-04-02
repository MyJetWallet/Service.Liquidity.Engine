using MyNoSqlServer.Abstractions;
using Service.Liquidity.Engine.Domain.Models.Portfolio;

namespace Service.Liquidity.Engine.Domain.NoSql
{
    public class PositionPortfolioNoSql: MyNoSqlDbEntity
    {
        public const string TableName = "myjetwallet-liquitityprovider-portfolio";

        public static string GeneratePartitionKey(string walletId) => $"walletId={walletId}";
        public static string GenerateRowKey(string positionId) => $"position:{positionId}";

        public PositionPortfolio Position { get; set; }

        public static PositionPortfolioNoSql Create(PositionPortfolio position)
        {
            return new PositionPortfolioNoSql()
            {
                PartitionKey = GeneratePartitionKey(position.WalletId),
                RowKey = GenerateRowKey(position.Id),
                Position = position
            };
        }
    }
}