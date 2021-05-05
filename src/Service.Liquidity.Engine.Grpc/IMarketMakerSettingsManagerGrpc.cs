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
        Task<MarketMakerSettings> GetMarketMakerSettingsAsync();

        [OperationContract]
        Task ChangeMarketMakerModeAsync(ChangeMarketMakerModeRequest request);

        [OperationContract]
        Task UpdateMarketMakerSettingsAsync(MarketMakerSettings request);


        [OperationContract]
        Task<GrpcList<LiquidityProviderInstrumentSettings>> GetLiquidityProviderInstrumentSettingsListAsync();

        [OperationContract]
        Task AddLiquidityProviderInstrumentSettingsAsync(LiquidityProviderInstrumentSettings setting);

        [OperationContract]
        Task UpdateLiquidityProviderInstrumentSettingsAsync(LiquidityProviderInstrumentSettings setting);

        [OperationContract]
        Task RemoveLiquidityProviderInstrumentSettingsAsync(RemoveSymbolRequest request);
    }
}