using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using Service.Liquidity.Engine.Domain.Models.Settings;
using Service.Liquidity.Engine.Grpc.Models;

namespace Service.Liquidity.Engine.Grpc
{
    [ServiceContract]
    public interface IHedgeSettingsManagerGrpc
    {
        Task<HedgeSettings> GetGlobalHedgeSettingsAsync();

        Task UpdateSettingsAsync(HedgeSettings request);

        Task<GrpcList<HedgeInstrumentSettings>> GetHedgeInstrumentSettingsListAsync();

        Task AddOrUpdateSettingsAsync(HedgeInstrumentSettings request);

        Task RemoveSettingsAsync(RemoveInstrumentHedgeSettingsRequest request);
    }
}