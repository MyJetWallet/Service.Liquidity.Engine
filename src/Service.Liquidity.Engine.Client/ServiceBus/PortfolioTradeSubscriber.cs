using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using MyJetWallet.Domain.ServiceBus;
using MyJetWallet.Domain.ServiceBus.Serializers;
using MyServiceBus.Abstractions;
using MyServiceBus.TcpClient;
using Service.Liquidity.Engine.Domain.Models.Portfolio;

namespace Service.Liquidity.Engine.Client.ServiceBus
{
    public class PortfolioTradeSubscriber: Subscriber<PortfolioTrade>
    {
        public PortfolioTradeSubscriber(MyServiceBusTcpClient client, string queueName, TopicQueueType queryType) 
            : base(client, PortfolioTrade.TopicName, queueName, queryType, bytes => bytes.ByteArrayToServiceBusContract<PortfolioTrade>(), true)
        {
        }
    }
}