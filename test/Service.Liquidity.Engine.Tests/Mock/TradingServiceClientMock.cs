using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using ME.Contracts.Api;
using ME.Contracts.Api.IncomingMessages;
using Status = ME.Contracts.Api.IncomingMessages.Status;

namespace Service.Liquidity.Engine.Tests.Mock
{
    public class TradingServiceClientMock : TradingService.TradingServiceClient
    {
        public List<MultiLimitOrder> CallList { get; set; } = new();

        public Func<MultiLimitOrder, MultiLimitOrderResponse> MultiLimitOrderCallback { get; set; }

        public override MultiLimitOrderResponse MultiLimitOrder(MultiLimitOrder request, CallOptions options)
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

        public override AsyncUnaryCall<MultiLimitOrderResponse> MultiLimitOrderAsync(MultiLimitOrder request,
            CallOptions options)
        {
            return new(Task.FromResult(MultiLimitOrder(request, new CallOptions())), null, null, null, null);
        }
    }
}