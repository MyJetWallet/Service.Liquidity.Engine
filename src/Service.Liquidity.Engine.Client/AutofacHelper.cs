using System.Collections.Generic;
using Autofac;
using DotNetCoreDecorators;
using MyJetWallet.Sdk.ServiceBus;
using MyServiceBus.Abstractions;
using MyServiceBus.TcpClient;
using Service.Liquidity.Engine.Domain.Models.Portfolio;
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
            builder.RegisterInstance(factory.GetWalletPortfolioGrpc()).As<IWalletPortfolioGrpc>().SingleInstance();
            builder.RegisterInstance(factory.GetHedgeSettingsManagerGrpc()).As<IHedgeSettingsManagerGrpc>().SingleInstance();
            builder.RegisterInstance(factory.GetExternalMarketsGrpc()).As<IExternalMarketsGrpc>().SingleInstance();
        }

        public static ContainerBuilder RegisterPortfolioTradeSubscriber(this ContainerBuilder builder, MyServiceBusTcpClient client, string queryName, TopicQueueType queryType)
        {
            builder.RegisterMyServiceBusSubscriberBatch<PortfolioTrade>(client, PortfolioTrade.TopicName, queryName,
                queryType);
            
            return builder;
        }

        public static ContainerBuilder RegisterPositionAssociationSubscriber(this ContainerBuilder builder, MyServiceBusTcpClient client, string queryName, TopicQueueType queryType)
        {
            builder.RegisterMyServiceBusSubscriberBatch<PositionAssociation>(client, PositionAssociation.TopicName, queryName, queryType);

            return builder;
        }

        public static ContainerBuilder RegisterPositionPortfolioSubscriber(this ContainerBuilder builder, MyServiceBusTcpClient client, string queryName, TopicQueueType queryType)
        {
            builder.RegisterMyServiceBusSubscriberBatch<PositionPortfolio>(client, PositionPortfolio.TopicName, queryName, queryType);
            
            return builder;
        }
    }
}