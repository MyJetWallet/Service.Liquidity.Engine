using System.Collections.Generic;
using Service.Liquidity.Engine.Domain.Models.Settings;
using Service.Liquidity.Engine.Domain.Services.Settings;

namespace Service.Liquidity.Engine.Tests
{
    public class MarketMakerSettingsAccessorMock: IMarketMakerSettingsAccessor
    {
        public MarketMakerSettings MmSettings { get; set; } = new(EngineMode.Disabled);

        public List<LiquidityProviderInstrumentSettings> LpSettings { get; set; } = new();

        public MarketMakerSettings GetMarketMakerSettings()
        {
            return MmSettings;
        }

        public List<LiquidityProviderInstrumentSettings> GetLiquidityProviderSettings()
        {
            return LpSettings;
        }
    }
}