using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Service.Liquidity.Engine.Domain.Models.Portfolio;
using Service.Liquidity.Engine.Domain.Services.Portfolio;
using Service.TradeHistory.ServiceBus;

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

        public Task ReportClosePosition(PositionPortfolio position)
        {
            ClosedPosition.Add(position);
            return Task.CompletedTask;
        }

        public Task ReportPositionUpdate(PositionPortfolio position)
        {
            return Task.CompletedTask;
        }

        public Task ReportPositionAssociation(PositionAssociation association)
        {
            return Task.CompletedTask;
        }
    }
}