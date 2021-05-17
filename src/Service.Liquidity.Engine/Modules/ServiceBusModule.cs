using Autofac;
using DotNetCoreDecorators;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Sdk.ServiceBus;
using MyServiceBus.Abstractions;
using Service.Liquidity.Engine.Domain.Models.Portfolio;
using Service.TradeHistory.Client;

namespace Service.Liquidity.Engine.Modules
{
    public class ServiceBusModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var serviceBusClient = builder.RegisterMyServiceBusTcpClient(Program.ReloadedSettings(e => e.SpotServiceBusHostPort), ApplicationEnvironment.HostName, Program.LogFactory);
                
            builder.RegisterTradeHistoryServiceBusClient(serviceBusClient, $"LiquidityEngine-{Program.Settings.ServiceBusQuerySuffix}", TopicQueueType.PermanentWithSingleConnection, true);

            builder
                .RegisterInstance(new MyServiceBusPublisher<PortfolioTrade>(serviceBusClient, PortfolioTrade.TopicName, false))
                .As<IPublisher<PortfolioTrade>>()
                .SingleInstance();

            builder
                .RegisterInstance(new MyServiceBusPublisher<PositionAssociation>(serviceBusClient, PositionAssociation.TopicName, false))
                .As<IPublisher<PositionAssociation>>()
                .SingleInstance();

            builder
                .RegisterInstance(new MyServiceBusPublisher<PositionPortfolio>(serviceBusClient, PositionPortfolio.TopicName, false))
                .As<IPublisher<PositionPortfolio>>()
                .SingleInstance();
        }
    }
}