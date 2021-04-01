using SimpleTrading.SettingsReader;

namespace Service.Liquidity.Engine.Settings
{
    [YamlAttributesOnly]
    public class SettingsModel
    {
        [YamlProperty("LiquidityEngine.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("LiquidityEngine.ExternalExchange.FTX.IsEnabled")]
        public bool FtxIsEnabled { get; set; }

        [YamlProperty("LiquidityEngine.ExternalExchange.FTX.WalletId")]
        public string FtxWalletId { get; set; }

        [YamlProperty("LiquidityEngine.ExternalExchange.FTX.InstrumentsOriginalSymbolToSymbol")]
        public string FtxInstrumentsOriginalSymbolToSymbol { get; set; }

        [YamlProperty("LiquidityEngine.BalancesGrpcServiceUrl")]
        public string BalancesGrpcServiceUrl { get; set; }

        [YamlProperty("LiquidityEngine.MyNoSqlReaderHostPort")]
        public string MyNoSqlReaderHostPort { get; set; }

        [YamlProperty("LiquidityEngine.MyNoSqlWriterUrl")]
        public string MyNoSqlWriterUrl { get; set; }

        [YamlProperty("LiquidityEngine.MatchingEngine.TradingGrpcServiceUrl")]
        public string MatchingEngineTradingServiceGrpcUrl { get; set; }

        [YamlProperty("LiquidityEngine.AccuracyToNormalizeDouble")]
        public int AccuracyToNormalizeDouble { get; set; }

        [YamlProperty("LiquidityEngine.SpotServiceBusHostPort")]
        public string SpotServiceBusHostPort { get; set; }

        [YamlProperty("LiquidityEngine.ServiceBusQuerySuffix")]
        public string ServiceBusQuerySuffix { get; set; }
    }
}