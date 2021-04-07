using System.ServiceModel;
using System.Threading.Tasks;
using Service.Liquidity.Engine.Domain.Models.ExternalMarkets;
using Service.Liquidity.Engine.Grpc.Models;

namespace Service.Liquidity.Engine.Grpc
{
    [ServiceContract]
    public interface IExternalMarketsGrpc
    {
        [OperationContract]
        Task<GrpcResponseWithData<GrpcList<string>>> GetExternalMarketListAsync();

        [OperationContract]
        Task<GrpcResponseWithData<GrpcList<AssetBalanceDto>>> GetBalancesAsync(SourceDto request);

        [OperationContract]
        Task<GrpcResponseWithData<GrpcList<ExchangeMarketInfo>>> GetInstrumentsAsync(SourceDto request);
    }
}