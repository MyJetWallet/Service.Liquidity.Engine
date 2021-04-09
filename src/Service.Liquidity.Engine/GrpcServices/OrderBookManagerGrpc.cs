using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyJetWallet.Domain.ExternalMarketApi.Models;
using Service.Liquidity.Engine.Domain.Services.OrderBooks;
using Service.Liquidity.Engine.Grpc;
using Service.Liquidity.Engine.Grpc.Models;

namespace Service.Liquidity.Engine.GrpcServices
{
    public class OrderBookManagerGrpc: IOrderBookManagerGrpc
    {
        private readonly IOrderBookManager _orderBookManager;

        public OrderBookManagerGrpc(IOrderBookManager orderBookManager)
        {
            _orderBookManager = orderBookManager;
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

        public async Task<GrpcResponseWithData<GrpcList<string>>> GetSourcesAsync()
        {
            var data = _orderBookManager.GetSources();
            return GrpcResponseWithData<GrpcList<string>>.Create(GrpcList<string>.Create(data.Result));
        }
    }
}