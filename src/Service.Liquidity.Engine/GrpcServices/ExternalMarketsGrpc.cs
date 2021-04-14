using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyJetWallet.Domain.ExternalMarketApi.Models;
using Service.Liquidity.Engine.Domain.Services.ExternalMarkets;
using Service.Liquidity.Engine.Grpc;
using Service.Liquidity.Engine.Grpc.Models;

namespace Service.Liquidity.Engine.GrpcServices
{
    public class ExternalMarketsGrpc : IExternalMarketsGrpc
    {
        private readonly IExternalMarketManager _externalMarketManager;
        private readonly IExternalBalanceCacheManager _externalBalanceCacheManager;

        public ExternalMarketsGrpc(IExternalMarketManager externalMarketManager, IExternalBalanceCacheManager externalBalanceCacheManager)
        {
            _externalMarketManager = externalMarketManager;
            _externalBalanceCacheManager = externalBalanceCacheManager;
        }

        public Task<GrpcResponseWithData<GrpcList<string>>> GetExternalMarketListAsync()
        {
            var data = _externalMarketManager.GetMarketNames();

            return GrpcResponseWithData<GrpcList<string>>.CreateTask(GrpcList<string>.Create(data));
        }

        public async Task<GrpcResponseWithData<GrpcList<AssetBalanceDto>>> GetBalancesAsync(SourceDto request)
        {
            var data = _externalBalanceCacheManager.GetBalances(request.Source);

            var result = data.Select(e => new AssetBalanceDto(e.Symbol, (double)e.Balance, (double)e.Free)).ToList();

            return GrpcResponseWithData<GrpcList<AssetBalanceDto>>.Create(GrpcList<AssetBalanceDto>.Create(result));
        }

        public async Task<GrpcResponseWithData<GrpcList<ExchangeMarketInfo>>> GetInstrumentsAsync(SourceDto request)
        {
            var data = _externalBalanceCacheManager.GetMarketInfo(request.Source);

            return GrpcResponseWithData<GrpcList<ExchangeMarketInfo>>.Create(GrpcList<ExchangeMarketInfo>.Create(data));
        }
    }
}