using System.ServiceModel;
using System.Threading.Tasks;
using Service.Liquidity.Engine.Domain.Models.Settings;
using Service.Liquidity.Engine.Grpc.Models;

namespace Service.Liquidity.Engine.Grpc
{
    [ServiceContract]
    public interface IMarketMakerSettingsManagerGrpc
    {
        [OperationContract]
        Task ChangeMarketMakerModeAsync(ChangeMarketMakerModeRequest request);

        [OperationContract]
        Task AddOrUpdateMirroringLiquiditySettingsAsync(MirroringLiquiditySettings setting);

        [OperationContract]
        Task RemoveMirroringLiquiditySettingsAsync(RemoveMirroringLiquiditySettingsRequest request);

        [OperationContract]
        Task<MarketMakerSettings> GetMarketMakerSettingsAsync();

        [OperationContract]
        Task<GrpcList<MirroringLiquiditySettings>> GetMirroringLiquiditySettingsListAsync();
    }
}