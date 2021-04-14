using System.Collections.Generic;
using MyJetWallet.Domain.ExternalMarketApi.Models;

namespace Service.Liquidity.Engine.Domain.Services.ExternalMarkets
{
    public interface IExternalBalanceCacheManager
    {
        ExchangeBalance GetBalances(string marketName, string symbol);

        List<ExchangeBalance> GetBalances(string marketName);

        ExchangeMarketInfo GetMarketInfo(string marketName, string symbol);

        List<ExchangeMarketInfo> GetMarketInfo(string marketName);
    }
}