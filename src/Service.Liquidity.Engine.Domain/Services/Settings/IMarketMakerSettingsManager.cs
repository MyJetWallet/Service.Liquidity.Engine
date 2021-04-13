﻿using System.Threading.Tasks;
using Service.Liquidity.Engine.Domain.Models.Settings;

namespace Service.Liquidity.Engine.Domain.Services.Settings
{
    public interface IMarketMakerSettingsManager
    {
        Task ChangeMarketMakerModeAsync(EngineMode mode);

        Task UpdateMirroringLiquiditySettingsAsync(MirroringLiquiditySettings setting);

        Task RemoveMirroringLiquiditySettingsAsync(string symbol, string walletName);

        Task UpdateMarketMakerSettingsAsync(MarketMakerSettings settings);

        Task AddMirroringLiquiditySettingsAsync(MirroringLiquiditySettings setting);
    }
}