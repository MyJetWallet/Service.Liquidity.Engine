using SimpleTrading.SettingsReader;

namespace Service.Liquidity.Engine.Settings
{
    [YamlAttributesOnly]
    public class SettingsModel
    {
        [YamlProperty("LiquidityEngine.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("LiquidityEngine.ExternalExchange.Ftx.FtxInstrumentsOriginalSymbolToSymbol")]
        public string FtxInstrumentsOriginalSymbolToSymbol { get; set; }
    }
}