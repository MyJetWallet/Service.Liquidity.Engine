using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Service.Liquidity.Engine.Domain.Models.OrderBooks;
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

        public Task<GrpcResponseWithData<LeOrderBook>> GetOrderBookAsync(GetOrderBookRequest request)
        {
            return GrpcResponseWithData<LeOrderBook>.CreateTask(_orderBookManager.GetOrderBook(request.Symbol, request.Source));
        }

        public Task<GrpcResponseWithData<Dictionary<string, GrpcList<string>>>> GetSourcesAndSymbolsAsync()
        {
            return GrpcResponseWithData<Dictionary<string, GrpcList<string>>>.CreateTask(
                _orderBookManager.GetSourcesAndSymbols().ToDictionary(
                    e => e.Key, 
                    e => GrpcList<string>.Create(e.Value)
                ));
        }

        public Task<GrpcResponseWithData<GrpcList<string>>> GetSymbolsAsync(GetSymbolsRequest request)
        {
            return GrpcResponseWithData<GrpcList<string>>.CreateTask(GrpcList<string>.Create(_orderBookManager.GetSymbols(request.Source)));
        }

        public Task<GrpcResponseWithData<GrpcList<string>>> GetSourcesWithSymbolAsync(GetSourcesWithSymbolRequest request)
        {
            return GrpcResponseWithData<GrpcList<string>>.CreateTask(GrpcList<string>.Create(_orderBookManager.GetSourcesWithSymbol(request.Symbol)));
        }

        public Task<GrpcResponseWithData<GrpcList<string>>> GetSourcesAsync()
        {
            return GrpcResponseWithData<GrpcList<string>>.CreateTask(GrpcList<string>.Create(_orderBookManager.GetSources()));
        }
    }
}