using System.Threading.Tasks;
using Service.Liquidity.Engine.Domain.Models.Settings;
using Service.Liquidity.Engine.Domain.Services.Settings;
using Service.Liquidity.Engine.Grpc;
using Service.Liquidity.Engine.Grpc.Models;

namespace Service.Liquidity.Engine.GrpcServices
{
    public class HedgeSettingsManagerGrpc: IHedgeSettingsManagerGrpc
    {
        private readonly IHedgeSettingsManager _settingsManager;
        private readonly IHedgeInstrumentSettingsManager _instrumentSettingsManager;

        public HedgeSettingsManagerGrpc(IHedgeSettingsManager settingsManager, IHedgeInstrumentSettingsManager instrumentSettingsManager)
        {
            _settingsManager = settingsManager;
            _instrumentSettingsManager = instrumentSettingsManager;
        }

        public Task<HedgeSettings> GetGlobalHedgeSettingsAsync()
        {
            var data = _settingsManager.GetGlobalHedgeSettings();

            return Task.FromResult(data);
        }

        public Task UpdateSettingsAsync(HedgeSettings request)
        {
            return _settingsManager.UpdateSettingsAsync(request);
        }

        public Task<GrpcList<HedgeInstrumentSettings>> GetHedgeInstrumentSettingsListAsync()
        {
            var data = _instrumentSettingsManager.GetHedgeInstrumentSettingsList();

            return Task.FromResult(GrpcList<HedgeInstrumentSettings>.Create(data));
        }

        public Task AddOrUpdateSettingsAsync(HedgeInstrumentSettings request)
        {
            return _instrumentSettingsManager.AddOrUpdateSettings(request);
        }

        public Task RemoveSettingsAsync(RemoveInstrumentHedgeSettingsRequest request)
        {
            return _instrumentSettingsManager.RemoveSettings(request.Symbol, request.WalletId);
        }
    }
}