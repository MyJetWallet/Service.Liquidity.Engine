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

        public HedgeSettingsManagerGrpc(IHedgeSettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
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
    }
}