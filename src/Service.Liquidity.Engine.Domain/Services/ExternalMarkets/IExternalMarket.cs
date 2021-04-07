using System.Collections.Generic;
using System.Threading.Tasks;
using MyJetWallet.Domain.Orders;
using Service.Liquidity.Engine.Domain.Models.ExternalMarkets;

namespace Service.Liquidity.Engine.Domain.Services.ExternalMarkets
{
    public interface IExternalMarket
    {
        public string GetName();
        public Task<double> GetBalance(string asset);
        public Task<Dictionary<string, double>> GetBalances();
        public Task<ExchangeMarketInfo> GetMarketInfo(string market);
        public Task<List<ExchangeMarketInfo>> GetMarketInfoListAsync();

        public Task<ExchangeTrade> MarketTrade(string market, OrderSide side, double volume, string referenceId);
    }
}