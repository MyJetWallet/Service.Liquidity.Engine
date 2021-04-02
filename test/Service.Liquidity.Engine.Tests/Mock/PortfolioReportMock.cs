using System.Threading.Tasks;
using Service.Liquidity.Engine.Domain.Models.Portfolio;
using Service.Liquidity.Engine.Domain.Services.Portfolio;
using Service.TradeHistory.ServiceBus;

namespace Service.Liquidity.Engine.Tests.Mock
{
    public class PortfolioReportMock: IPortfolioReport
    {
        public Task ReportInternalTrade(PortfolioTrade tradeMessage)
        {
            return Task.CompletedTask;
        }

        public Task ReportClosePosition(PositionPortfolio position)
        {
            return Task.CompletedTask;
        }

        public Task ReportPositionAssociation(PositionAssociation association)
        {
            return Task.CompletedTask;
        }
    }
}