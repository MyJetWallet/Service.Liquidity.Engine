using MyNoSqlServer.Abstractions;
using Service.Liquidity.Engine.Domain.Models.Settings;

namespace Service.Liquidity.Engine.Domain.NoSql
{
    public class SettingsMarketMakerNoSql : MyNoSqlDbEntity
    {
        public const string TableName = "myjetwallet-liquitityprovider-settings";

        public static string GeneratePartitionKey() => "market-maker";
        public static string GenerateRowKey() => "general";

        public MarketMakerSettings Settings { get; set; }

        public static SettingsMarketMakerNoSql Create(MarketMakerSettings settings)
        {
            return new SettingsMarketMakerNoSql()
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(),
                Settings = settings
            };
        }
    }
}