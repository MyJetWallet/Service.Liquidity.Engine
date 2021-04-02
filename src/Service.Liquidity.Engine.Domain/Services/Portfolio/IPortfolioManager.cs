using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Liquidity.Engine.Domain.Models.Portfolio;
using Service.TradeHistory.Domain.Models;
using Service.TradeHistory.ServiceBus;

namespace Service.Liquidity.Engine.Domain.Services.Portfolio
{
    public interface IPortfolioManager
    {
        ValueTask RegisterLocalTrades(List<WalletTradeMessage> trades);

        Task<List<PositionPortfolio>> GetPortfolio();
    }
}