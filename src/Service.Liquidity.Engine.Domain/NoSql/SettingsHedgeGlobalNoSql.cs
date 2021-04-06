using MyNoSqlServer.Abstractions;
using Service.Liquidity.Engine.Domain.Models.Settings;

namespace Service.Liquidity.Engine.Domain.NoSql
{
    public class SettingsHedgeGlobalNoSql : MyNoSqlDbEntity
    {
        public const string TableName = "myjetwallet-liquitityprovider-settings";

        public static string GeneratePartitionKey() => "hedge";
        public static string GenerateRowKey() => "general";

        public HedgeSettings Settings { get; set; }

        public static SettingsHedgeGlobalNoSql Create(HedgeSettings settings)
        {
            return new SettingsHedgeGlobalNoSql()
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(),
                Settings = settings
            };
        }
    }
}