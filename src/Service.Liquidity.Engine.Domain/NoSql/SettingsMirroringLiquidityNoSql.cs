using System.Collections.Generic;
using MyNoSqlServer.Abstractions;
using Service.Liquidity.Engine.Domain.Models.Settings;

namespace Service.Liquidity.Engine.Domain.NoSql
{
    public class SettingsMirroringLiquidityNoSql : MyNoSqlDbEntity
    {
        public const string TableName = "myjetwallet-liquitityprovider-settings";

        public static string GeneratePartitionKey() => "mirror-liquidity";
        public static string GenerateRowKey(string symbol, string walletName) => $"{symbol}::{walletName}";

        public MirroringLiquiditySettings Settings { get; set; }

        public static SettingsMirroringLiquidityNoSql Create(MirroringLiquiditySettings settings)
        {
            return new SettingsMirroringLiquidityNoSql()
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(settings.InstrumentSymbol, settings.WalletName),
                Settings = settings
            };
        }
    }
}