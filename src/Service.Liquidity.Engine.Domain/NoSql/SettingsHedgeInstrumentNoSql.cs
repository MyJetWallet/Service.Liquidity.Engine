using MyNoSqlServer.Abstractions;
using Service.Liquidity.Engine.Domain.Models.Settings;

namespace Service.Liquidity.Engine.Domain.NoSql
{
    public class SettingsHedgeInstrumentNoSql : MyNoSqlDbEntity
    {
        public const string TableName = "myjetwallet-liquitityprovider-settings";

        public static string GeneratePartitionKey() => "hedge-instrument";
        public static string GenerateRowKey(string symbol, string walletId) => $"{symbol}::{walletId}";

        public HedgeInstrumentSettings Settings { get; set; }

        public static SettingsHedgeInstrumentNoSql Create(HedgeInstrumentSettings settings)
        {
            return new SettingsHedgeInstrumentNoSql()
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(settings.InstrumentSymbol, settings.WalletId),
                Settings = settings
            };
        }
    }
}