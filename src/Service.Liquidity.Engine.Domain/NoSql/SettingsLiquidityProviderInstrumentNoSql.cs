using MyNoSqlServer.Abstractions;
using Service.Liquidity.Engine.Domain.Models.Settings;

namespace Service.Liquidity.Engine.Domain.NoSql
{
    public class SettingsLiquidityProviderInstrumentNoSql : MyNoSqlDbEntity
    {
        public const string TableName = "myjetwallet-liquitityprovider-settings";

        public static string GeneratePartitionKey() => "liquidity-provider-instrument";
        public static string GenerateRowKey(string symbol) => symbol;

        public LiquidityProviderInstrumentSettings Settings { get; set; }

        public static SettingsLiquidityProviderInstrumentNoSql Create(LiquidityProviderInstrumentSettings settings)
        {
            return new SettingsLiquidityProviderInstrumentNoSql()
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(settings.Symbol),
                Settings = settings
            };
        }
    }
}