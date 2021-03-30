using System.Collections.Generic;
using Service.Liquidity.Engine.Domain.Models.Settings;
using Service.Liquidity.Engine.Domain.Services.Settings;

namespace Service.Liquidity.Engine.Tests
{
    public class MarketMakerSettingsAccessorMock: IMarketMakerSettingsAccessor
    {
        public MarketMakerSettings MmSettings { get; set; } = new(EngineMode.Disabled);

        public List<MirroringLiquiditySettings> MlSettings { get; set; } = new();

        public MarketMakerSettings GetMarketMakerSettings()
        {
            return MmSettings;
        }

        public List<MirroringLiquiditySettings> GetMirroringLiquiditySettingsList()
        {
            return MlSettings;
        }
    }
}