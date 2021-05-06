using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ME.Contracts.Api;
using ME.Contracts.Api.IncomingMessages;

namespace Service.Liquidity.Engine.Tests
{
    public class TradingServiceClientMock : TradingService.TradingServiceClient
    {
        public List<MultiLimitOrder> CallList { get; set; } = new();

        public Func<MultiLimitOrder, MultiLimitOrderResponse> MultiLimitOrderCallback { get; set; }

        public MarketOrderResponse MarketOrder(MarketOrder request, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<MarketOrderResponse> MarketOrderAsync(MarketOrder request, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public LimitOrderResponse LimitOrder(LimitOrder request, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<LimitOrderResponse> LimitOrderAsync(LimitOrder request, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public LimitOrderCancelResponse CancelLimitOrder(LimitOrderCancel request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<LimitOrderCancelResponse> CancelLimitOrderAsync(LimitOrderCancel request, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public MultiLimitOrderResponse MultiLimitOrder(MultiLimitOrder request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            CallList.Add(request);

            if (MultiLimitOrderCallback != null)
            {
                return MultiLimitOrderCallback.Invoke(request);
            }

            var resp = new MultiLimitOrderResponse()
            {
                Id = request.Id,
                MessageId = request.MessageId,
                AssetPairId = request.AssetPairId,
                Status = Status.Ok,
                WalletVersion = 0
            };

            foreach (var order in request.Orders)
            {
                resp.Statuses.Add(new MultiLimitOrderResponse.Types.OrderStatus()
                {
                    Id = order.Id,
                    MatchingEngineId = order.Id,
                    Price = order.Price,
                    Status = Status.Ok,
                    Volume = order.Volume
                });
            }

            return resp;
        }

        public Task<MultiLimitOrderResponse> MultiLimitOrderAsync(MultiLimitOrder request, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult(MultiLimitOrder(request));
        }
    }
}