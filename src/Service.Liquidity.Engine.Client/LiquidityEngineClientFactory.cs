using System;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using JetBrains.Annotations;
using MyJetWallet.Sdk.GrpcMetrics;
using ProtoBuf.Grpc.Client;
using Service.Liquidity.Engine.Grpc;

namespace Service.Liquidity.Engine.Client
{
    [UsedImplicitly]
    public class LiquidityEngineClientFactory
    {
        private readonly CallInvoker _channel;

        public LiquidityEngineClientFactory(string assetsDictionaryGrpcServiceUrl)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var channel = GrpcChannel.ForAddress(assetsDictionaryGrpcServiceUrl);
            _channel = channel.Intercept(new PrometheusMetricsInterceptor());
        }

        public ILpWalletManagerGrpc GetLpWalletManagerGrpc() => _channel.CreateGrpcService<ILpWalletManagerGrpc>();
        public IMarketMakerSettingsManagerGrpc GetMarketMakerSettingsManagerGrpc() => _channel.CreateGrpcService<IMarketMakerSettingsManagerGrpc>();
        public IOrderBookManagerGrpc GetOrderBookManagerGrpc() => _channel.CreateGrpcService<IOrderBookManagerGrpc>();
        public IWalletPortfolioGrpc GetWalletPortfolioGrpc() => _channel.CreateGrpcService<IWalletPortfolioGrpc>();
        public IHedgeSettingsManagerGrpc GetHedgeSettingsManagerGrpc() => _channel.CreateGrpcService<IHedgeSettingsManagerGrpc>();
        public IExternalMarketsGrpc GetExternalMarketsGrpc() => _channel.CreateGrpcService<IExternalMarketsGrpc>();
    }
}
