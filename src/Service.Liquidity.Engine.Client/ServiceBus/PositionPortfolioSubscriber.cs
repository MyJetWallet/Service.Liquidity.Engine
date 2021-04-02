using MyJetWallet.Domain.ServiceBus;
using MyJetWallet.Domain.ServiceBus.Serializers;
using MyServiceBus.Abstractions;
using MyServiceBus.TcpClient;
using Service.Liquidity.Engine.Domain.Models.Portfolio;

namespace Service.Liquidity.Engine.Client.ServiceBus
{
    public class PositionPortfolioSubscriber : Subscriber<PositionPortfolio>
    {
        public PositionPortfolioSubscriber(MyServiceBusTcpClient client, string queueName, TopicQueueType queryType)
            : base(client, PortfolioTrade.TopicName, queueName, queryType, bytes => bytes.ByteArrayToServiceBusContract<PositionPortfolio>(), true)
        {
        }
    }
}