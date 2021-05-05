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
        [OperationContract]
        Task<HedgeSettings> GetGlobalHedgeSettingsAsync();

        [OperationContract]
        Task UpdateSettingsAsync(HedgeSettings request);
    }
}