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

        public ExternalMarketsGrpc(IExternalMarketManager externalMarketManager)
        {
            _externalMarketManager = externalMarketManager;
        }

        public Task<GrpcResponseWithData<GrpcList<string>>> GetExternalMarketListAsync()
        {
            var data = _externalMarketManager.GetMarketNames();

            return GrpcResponseWithData<GrpcList<string>>.CreateTask(GrpcList<string>.Create(data));
        }

        public async Task<GrpcResponseWithData<GrpcList<AssetBalanceDto>>> GetBalancesAsync(SourceDto request)
        {
            var market = _externalMarketManager.GetExternalMarketByName(request.Source);

            if (market == null)
                return GrpcResponseWithData<GrpcList<AssetBalanceDto>>.Create(GrpcList<AssetBalanceDto>.Create(new List<AssetBalanceDto>()));

            var data = await market.GetBalancesAsync();

            var result = data.Balances.Select(e => new AssetBalanceDto(e.Symbol, (double)e.Balance, (double)e.Free)).ToList();

            return GrpcResponseWithData<GrpcList<AssetBalanceDto>>.Create(GrpcList<AssetBalanceDto>.Create(result));
        }

        public async Task<GrpcResponseWithData<GrpcList<ExchangeMarketInfo>>> GetInstrumentsAsync(SourceDto request)
        {
            var market = _externalMarketManager.GetExternalMarketByName(request.Source);

            if (market == null)
            {
                return GrpcResponseWithData<GrpcList<ExchangeMarketInfo>>.Create(GrpcList<ExchangeMarketInfo>.Create(new List<ExchangeMarketInfo>()));
            }

            var data = await market.GetMarketInfoListAsync();

            return GrpcResponseWithData<GrpcList<ExchangeMarketInfo>>.Create(GrpcList<ExchangeMarketInfo>.Create(data.Infos));
        }
    }
}