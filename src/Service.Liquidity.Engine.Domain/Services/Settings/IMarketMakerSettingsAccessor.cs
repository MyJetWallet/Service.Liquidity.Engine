using System.Collections.Generic;
using Service.Liquidity.Engine.Domain.Models.Settings;

namespace Service.Liquidity.Engine.Domain.Services.Settings
{
    public interface IMarketMakerSettingsAccessor
    {
        MarketMakerSettings GetMarketMakerSettings();

        List<MirroringLiquiditySettings> GetMirroringLiquiditySettingsList();

        List<LiquidityProviderInstrumentSettings> GetLiquidityProviderSettings();
    }
}