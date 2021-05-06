using System.Collections.Generic;
using MyJetWallet.Sdk.Service;
using MyYamlParser;

namespace Service.Liquidity.Engine.Settings
{
    public class SettingsModel
    {
        [YamlProperty("LiquidityEngine.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("LiquidityEngine.BalancesGrpcServiceUrl")]
        public string BalancesGrpcServiceUrl { get; set; }

        [YamlProperty("LiquidityEngine.MyNoSqlReaderHostPort")]
        public string MyNoSqlReaderHostPort { get; set; }

        [YamlProperty("LiquidityEngine.MyNoSqlWriterUrl")]
        public string MyNoSqlWriterUrl { get; set; }

        [YamlProperty("LiquidityEngine.MatchingEngineApiGrpcServiceUrl")]
        public string MatchingEngineApiGrpcServiceUrl { get; set; }

        [YamlProperty("LiquidityEngine.AccuracyToNormalizeDouble")]
        public int AccuracyToNormalizeDouble { get; set; }

        [YamlProperty("LiquidityEngine.SpotServiceBusHostPort")]
        public string SpotServiceBusHostPort { get; set; }

        [YamlProperty("LiquidityEngine.ServiceBusQuerySuffix")]
        public string ServiceBusQuerySuffix { get; set; }

        [YamlProperty("LiquidityEngine.ZipkinUrl")]
        public string ZipkinUrl { get; set; }

        [YamlProperty("LiquidityEngine.ExternalExchange")]
        public Dictionary<string, ExternalExchange> ExternalExchange { get; set; }

        [YamlProperty("LiquidityEngine.ElkLogs")]
        public LogElkSettings ElkLogs { get; set; }
    }

    public class ExternalExchange
    {
        [YamlProperty("IsEnabled")]
        public bool IsEnabled { get; set; }

        [YamlProperty("GrpcUrl")]
        public string GrpcUrl { get; set; }
    }

}