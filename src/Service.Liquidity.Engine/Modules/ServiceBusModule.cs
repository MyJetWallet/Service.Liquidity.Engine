using Autofac;
using DotNetCoreDecorators;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Sdk.ServiceBus;
using MyServiceBus.Abstractions;
using Service.BalanceHistory.Client;
using Service.Liquidity.Engine.Domain.Models.Portfolio;

namespace Service.Liquidity.Engine.Modules
{
    public class ServiceBusModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var serviceBusClient = builder.RegisterMyServiceBusTcpClient(Program.ReloadedSettings(e => e.SpotServiceBusHostPort), Program.LogFactory);
                
            builder.RegisterTradeHistoryServiceBusClient(serviceBusClient, $"LiquidityEngine-{Program.Settings.ServiceBusQuerySuffix}", TopicQueueType.PermanentWithSingleConnection, true);

            builder.RegisterMyServiceBusPublisher<PortfolioTrade>(serviceBusClient, PortfolioTrade.TopicName, false);

            builder.RegisterMyServiceBusPublisher<PositionAssociation>(serviceBusClient, PositionAssociation.TopicName, false);

            builder.RegisterMyServiceBusPublisher<PositionPortfolio>(serviceBusClient, PositionPortfolio.TopicName, false);
        }
    }
}