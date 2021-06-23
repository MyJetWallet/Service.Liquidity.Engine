using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Liquidity.Engine.Domain.Models.Portfolio;
using Service.Liquidity.Engine.Domain.Services.Portfolio;

namespace Service.Liquidity.Engine.Tests.Mock
{
    public class PortfolioReportMock: IPortfolioReport
    {
        public List<PositionPortfolio> ClosedPosition = new();

        public Task ReportInternalTrade(PortfolioTrade tradeMessage)
        {
            return Task.CompletedTask;
        }

        public Task ReportExternalTrade(PortfolioTrade trade)
        {
            return Task.CompletedTask;
        }

        public Task ReportPositionUpdate(PositionPortfolio position)
        {
            if (!position.IsOpen)
                ClosedPosition.Add(position);

            return Task.CompletedTask;
        }

        public Task ReportPositionAssociation(PositionAssociation association)
        {
            return Task.CompletedTask;
        }
    }
}