using System.Threading.Tasks;
using Service.Liquidity.Engine.Domain.Models.Portfolio;

namespace Service.Liquidity.Engine.Domain.Services.Portfolio
{
    public interface IPortfolioReport
    {
        Task ReportInternalTrade(PortfolioTrade tradeMessage);
        Task ReportExternalTrade(PortfolioTrade trade);
        
        Task ReportPositionUpdate(PositionPortfolio position);
        
        Task ReportPositionAssociation(PositionAssociation association);
    }
}