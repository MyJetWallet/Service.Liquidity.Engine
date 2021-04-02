using System.Threading.Tasks;
using DotNetCoreDecorators;
using MyJetWallet.Domain.Prices;
using MyJetWallet.Domain.ServiceBus.Serializers;
using MyServiceBus.TcpClient;
using Service.Liquidity.Engine.Domain.Models.Portfolio;

namespace Service.Liquidity.Engine.Domain.Services.ServiceBus
{
    public class PortfolioTradePublisher : IPublisher<PortfolioTrade>
    {
        private readonly MyServiceBusTcpClient _client;

        public PortfolioTradePublisher(MyServiceBusTcpClient client)
        {
            this._client = client;
            this._client.CreateTopicIfNotExists(PortfolioTrade.TopicName);
        }

        public async ValueTask PublishAsync(PortfolioTrade valueToPublish)
        {
            await this._client.PublishAsync(PortfolioTrade.TopicName, valueToPublish.ServiceBusContractToByteArray(), true);
        }
    }
    
}