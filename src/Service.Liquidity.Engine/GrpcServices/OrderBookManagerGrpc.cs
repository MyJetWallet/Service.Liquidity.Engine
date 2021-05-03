using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyJetWallet.Domain.ExternalMarketApi.Models;
using Service.Liquidity.Engine.Domain.Models.LiquidityProvider;
using Service.Liquidity.Engine.Domain.Services.LiquidityProvider;
using Service.Liquidity.Engine.Domain.Services.OrderBooks;
using Service.Liquidity.Engine.Grpc;
using Service.Liquidity.Engine.Grpc.Models;

namespace Service.Liquidity.Engine.GrpcServices
{
    public class OrderBookManagerGrpc: IOrderBookManagerGrpc
    {
        private readonly IOrderBookManager _orderBookManager;
        private readonly IAggregateLiquidityProviderOrders _aggregateLiquidityProviderOrders;

        public OrderBookManagerGrpc(IOrderBookManager orderBookManager, IAggregateLiquidityProviderOrders aggregateLiquidityProviderOrders)
        {
            _orderBookManager = orderBookManager;
            _aggregateLiquidityProviderOrders = aggregateLiquidityProviderOrders;
        }

        public async Task<GrpcResponseWithData<LeOrderBook>> GetOrderBookAsync(GetOrderBookRequest request)
        {
            var data = await _orderBookManager.GetOrderBook(request.Symbol, request.Source);

            return GrpcResponseWithData<LeOrderBook>.Create(data);
        }

        public async Task<GrpcResponseWithData<Dictionary<string, GrpcList<string>>>> GetSourcesAndSymbolsAsync()
        {
            var data = await _orderBookManager.GetSourcesAndSymbols();
                var result = data.ToDictionary(
                e => e.Key,
                e => GrpcList<string>.Create(e.Value)
            );
            return GrpcResponseWithData<Dictionary<string, GrpcList<string>>>.Create(result);
        }

        public async Task<GrpcResponseWithData<GrpcList<string>>> GetSymbolsAsync(GetSymbolsRequest request)
        {
            var data = await _orderBookManager.GetSymbols(request.Source);
            return GrpcResponseWithData<GrpcList<string>>.Create(GrpcList<string>.Create(data));
        }

        public async Task<GrpcResponseWithData<GrpcList<string>>> GetSourcesWithSymbolAsync(GetSourcesWithSymbolRequest request)
        {
            var data = await _orderBookManager.GetSourcesWithSymbol(request.Symbol);
            return GrpcResponseWithData<GrpcList<string>>.Create(GrpcList<string>.Create(data));
        }

        public Task<GrpcResponseWithData<GrpcList<string>>> GetSourcesAsync()
        {
            var data = _orderBookManager.GetSources();
            return GrpcResponseWithData<GrpcList<string>>.CreateTask(GrpcList<string>.Create(data.Result));
        }

        public Task<GrpcResponseWithData<GrpcList<LpOrder>>> GetLiquidityProviderLastOrdersAsync(GetLiquidityProviderLastOrderRequest request)
        {
            var data = _aggregateLiquidityProviderOrders.GetCurrentOrders(request.BrokerId, request.Symbol);
            return GrpcResponseWithData<GrpcList<LpOrder>>.CreateTask(GrpcList<LpOrder>.Create(data));
        }
    }
}