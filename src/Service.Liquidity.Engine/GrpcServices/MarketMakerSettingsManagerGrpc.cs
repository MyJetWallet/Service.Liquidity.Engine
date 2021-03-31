﻿using System.Threading.Tasks;
using Service.Liquidity.Engine.Domain.Models.Settings;
using Service.Liquidity.Engine.Domain.Services.Settings;
using Service.Liquidity.Engine.Grpc;
using Service.Liquidity.Engine.Grpc.Models;

namespace Service.Liquidity.Engine.GrpcServices
{
    public class MarketMakerSettingsManagerGrpc: IMarketMakerSettingsManagerGrpc
    {
        private readonly IMarketMakerSettingsAccessor _accessor;
        private readonly IMarketMakerSettingsManager _manager;

        public MarketMakerSettingsManagerGrpc(IMarketMakerSettingsAccessor accessor, IMarketMakerSettingsManager manager)
        {
            _accessor = accessor;
            _manager = manager;
        }

        public Task ChangeMarketMakerModeAsync(ChangeMarketMakerModeRequest request)
        {
            return _manager.ChangeMarketMakerModeAsync(request.Mode);
        }

        public Task UpdateMarketMakerSettingsAsync(MarketMakerSettings request)
        {
            return _manager.UpdateMarketMakerSettingsAsync(request);
        }

        public Task AddOrUpdateMirroringLiquiditySettingsAsync(MirroringLiquiditySettings setting)
        {
            return _manager.AddOrUpdateMirroringLiquiditySettingsAsync(setting);
        }

        public Task RemoveMirroringLiquiditySettingsAsync(RemoveMirroringLiquiditySettingsRequest request)
        {
            return _manager.RemoveMirroringLiquiditySettingsAsync(request.Symbol, request.WalletName);
        }

        public Task<MarketMakerSettings> GetMarketMakerSettingsAsync()
        {
            var data = _accessor.GetMarketMakerSettings();
            return Task.FromResult(data);
        }

        public Task<GrpcList<MirroringLiquiditySettings>> GetMirroringLiquiditySettingsListAsync()
        {
            var data = _accessor.GetMirroringLiquiditySettingsList();

            return Task.FromResult(GrpcList<MirroringLiquiditySettings>.Create(data));
        }
    }
}