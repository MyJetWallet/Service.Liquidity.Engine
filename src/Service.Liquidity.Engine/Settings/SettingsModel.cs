﻿using System.Collections.Generic;
using SimpleTrading.SettingsReader;

namespace Service.Liquidity.Engine.Settings
{
    [YamlAttributesOnly]
    public class SettingsModel
    {
        [YamlProperty("LiquidityEngine.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("LiquidityEngine.ExternalExchange.FTXSimulation.IsEnabled")]
        public bool FtxSimulateIsEnabled { get; set; }

        [YamlProperty("LiquidityEngine.ExternalExchange.FTXSimulation.ExchangeGrpcUrl")]
        public string FtxSimulateExchangeGrpcUrl { get; set; }

        [YamlProperty("LiquidityEngine.ExternalExchange.FTX.IsEnabled")]
        public bool FtxIsEnabled { get; set; }

        [YamlProperty("LiquidityEngine.ExternalExchange.FTX.ExchangeGrpcUrl")]
        public string FtxExchangeGrpcUrl { get; set; }

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

        [YamlProperty("LiquidityEngine.ZipkinUrl")]
        public string ZipkinUrl { get; set; }


    }

}