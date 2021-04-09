using System.Collections.Generic;
using System.Threading.Tasks;
using MyJetWallet.Domain.ExternalMarketApi.Models;
using Service.Liquidity.Engine.Domain.Models.Portfolio;
using Service.TradeHistory.ServiceBus;

namespace Service.Liquidity.Engine.Domain.Services.Portfolio
{
    public interface IPortfolioManager
    {
        ValueTask RegisterLocalTradesAsync(List<WalletTradeMessage> trades);

        Task RegisterHedgeTradeAsync(ExchangeTrade trade);

        Task<List<PositionPortfolio>> GetPortfolioAsync();
    }
}