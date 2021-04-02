using System.Threading.Tasks;
using Service.Liquidity.Engine.Domain.Models.Portfolio;
using Service.TradeHistory.ServiceBus;

namespace Service.Liquidity.Engine.Domain.Services.Portfolio
{
    public interface IPortfolioReport
    {
        Task ReportInternalTrade(PortfolioTrade tradeMessage);
        Task ReportClosePosition(PositionPortfolio position);
        Task ReportPositionAssociation(PositionAssociation association);
    }
}