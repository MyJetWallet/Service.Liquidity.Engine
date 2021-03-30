namespace Service.Liquidity.Engine.Domain.Models.Settings
{
    public class MarketMakerSettings
    {
        public EngineMode Mode { get; set; }



    }

    public class MirroringLiquiditySettings
    {
        public EngineMode Mode { get; set; }

        public string InstrumentSymbol { get; set; }

        public string ExternalMarket { get; set; }

        public string ExternalSymbol { get; set; }

        public double Markup { get; set; }

        public string WalletName { get; set; }
    }
}