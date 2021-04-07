using System.Threading.Tasks;
using DotNetCoreDecorators;
using MyJetWallet.Domain.ServiceBus.Serializers;
using MyServiceBus.TcpClient;
using Service.Liquidity.Engine.Domain.Models.Portfolio;

namespace Service.Liquidity.Engine.Domain.Services.ServiceBus
{
    public class PositionPortfolioPublisher : IPublisher<PositionPortfolio>
    {
        private readonly MyServiceBusTcpClient _client;

        public PositionPortfolioPublisher(MyServiceBusTcpClient client)
        {
            this._client = client;
            this._client.CreateTopicIfNotExists(PositionPortfolio.TopicName);
        }

        public async ValueTask PublishAsync(PositionPortfolio valueToPublish)
        {
            await this._client.PublishAsync(PositionPortfolio.TopicName, valueToPublish.ServiceBusContractToByteArray(), false);
        }
    }
}