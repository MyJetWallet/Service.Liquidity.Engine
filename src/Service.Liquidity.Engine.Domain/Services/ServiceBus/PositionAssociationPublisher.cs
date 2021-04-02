using System.Threading.Tasks;
using DotNetCoreDecorators;
using MyJetWallet.Domain.ServiceBus.Serializers;
using MyServiceBus.TcpClient;
using Service.Liquidity.Engine.Domain.Models.Portfolio;

namespace Service.Liquidity.Engine.Domain.Services.ServiceBus
{
    public class PositionAssociationPublisher : IPublisher<PositionAssociation>
    {
        private readonly MyServiceBusTcpClient _client;

        public PositionAssociationPublisher(MyServiceBusTcpClient client)
        {
            this._client = client;
            this._client.CreateTopicIfNotExists(PositionAssociation.TopicName);
        }

        public async ValueTask PublishAsync(PositionAssociation valueToPublish)
        {
            await this._client.PublishAsync(PositionAssociation.TopicName, valueToPublish.ServiceBusContractToByteArray(), true);
        }
    }
}