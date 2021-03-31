using Autofac;
using Service.Liquidity.Engine.Grpc;
// ReSharper disable UnusedMember.Global

namespace Service.Liquidity.Engine.Client
{
    public static class AutofacHelper
    {
        public static void RegisterLiquidityEngineClient(this ContainerBuilder builder, string liquidityEngineGrpcServiceUrl)
        {
            var factory = new LiquidityEngineClientFactory(liquidityEngineGrpcServiceUrl);

            builder.RegisterInstance(factory.GetLpWalletManagerGrpc()).As<ILpWalletManagerGrpc>().SingleInstance();
            builder.RegisterInstance(factory.GetMarketMakerSettingsManagerGrpc()).As<IMarketMakerSettingsManagerGrpc>().SingleInstance();
            builder.RegisterInstance(factory.GetOrderBookManagerGrpc()).As<IOrderBookManagerGrpc>().SingleInstance();
        }
    }
}