using System;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Service.Liquidity.Engine.Domain.Models.Portfolio;

namespace Service.Liquidity.Engine.Domain.Services.Portfolio
{
    public class PortfolioReport : IPortfolioReport
    {
        private readonly ILogger<PortfolioReport> _logger;
        private readonly IPublisher<PortfolioTrade> _tradePublisher;
        private readonly IPublisher<PositionAssociation> _associationPublisher;
        private readonly IPublisher<PositionPortfolio> _positionPublisher;

        public PortfolioReport(
            ILogger<PortfolioReport> logger,
            IPublisher<PortfolioTrade> tradePublisher,
            IPublisher<PositionAssociation> associationPublisher,
            IPublisher<PositionPortfolio> positionPublisher)
        {
            _logger = logger;
            _tradePublisher = tradePublisher;
            _associationPublisher = associationPublisher;
            _positionPublisher = positionPublisher;
        }

        public async Task ReportInternalTrade(PortfolioTrade trade)
        {
            try
            {
                await _tradePublisher.PublishAsync(trade);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot publish PortfolioTrade: {jsonText}", JsonConvert.SerializeObject(trade));
            }
        }

        public async Task ReportExternalTrade(PortfolioTrade trade)
        {
            try
            {
                await _tradePublisher.PublishAsync(trade);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot publish PortfolioTrade: {jsonText}", JsonConvert.SerializeObject(trade));
            }
        }

        public async Task ReportPositionUpdate(PositionPortfolio position)
        {
            try
            {
                await _positionPublisher.PublishAsync(position);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot publish PositionPortfolio: {jsonText}", JsonConvert.SerializeObject(position));
            }
        }

        public async Task ReportPositionAssociation(PositionAssociation association)
        {
            try
            {
                await _associationPublisher.PublishAsync(association);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot publish PositionAssociation: {jsonText}", JsonConvert.SerializeObject(association));
            }
        }
    }
}