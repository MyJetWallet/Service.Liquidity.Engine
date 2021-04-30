using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using MyJetWallet.Domain.ExternalMarketApi.Models;
using Service.Liquidity.Engine.Domain.Models.LiquidityProvider;
using Service.Liquidity.Engine.Grpc.Models;

namespace Service.Liquidity.Engine.Grpc
{
    [ServiceContract]
    public interface IOrderBookManagerGrpc
    {
        [OperationContract]
        Task<GrpcResponseWithData<LeOrderBook>> GetOrderBookAsync(GetOrderBookRequest request);

        [OperationContract]
        Task<GrpcResponseWithData<Dictionary<string, GrpcList<string>>>> GetSourcesAndSymbolsAsync();

        [OperationContract]
        Task<GrpcResponseWithData<GrpcList<string>>> GetSymbolsAsync(GetSymbolsRequest request);

        [OperationContract]
        Task<GrpcResponseWithData<GrpcList<string>>> GetSourcesWithSymbolAsync(GetSourcesWithSymbolRequest request);

        [OperationContract]
        Task<GrpcResponseWithData<GrpcList<string>>> GetSourcesAsync();

        [OperationContract]
        Task<GrpcResponseWithData<GrpcList<LpOrder>>> GetLiquidityProviderLastOrdersAsync();
    }
}