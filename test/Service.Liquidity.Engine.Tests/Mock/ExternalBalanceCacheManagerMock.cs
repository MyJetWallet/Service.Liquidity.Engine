using System.Collections.Generic;
using MyJetWallet.Domain.ExternalMarketApi.Models;
using Service.Liquidity.Engine.Domain.Services.ExternalMarkets;

namespace Service.Liquidity.Engine.Tests.Mock
{
    public class ExternalBalanceCacheManagerMock: IExternalBalanceCacheManager
    {
        public Dictionary<string, Dictionary<string, ExchangeBalance>> Balances { get; set; } = new();
        public readonly Dictionary<string, Dictionary<string, ExchangeMarketInfo>> Markets = new();

        public ExchangeBalance GetBalances(string marketName, string symbol)
        {
            if (!Balances.TryGetValue(marketName, out var balancesData) ||
                !balancesData.TryGetValue(symbol, out var data))
            {
                return new ExchangeBalance() { Symbol = symbol, Balance = 0, Free = 0 };
            }

            return data;
        }

        public List<ExchangeBalance> GetBalances(string marketName)
        {
            throw new System.NotImplementedException();
        }

        public ExchangeMarketInfo GetMarketInfo(string marketName, string symbol)
        {
            if (!Markets.TryGetValue(marketName, out var marketData) ||
                !marketData.TryGetValue(symbol, out var data))
            {
                return null;
            }

            return data;
        }

        public List<ExchangeMarketInfo> GetMarketInfo(string marketName)
        {
            throw new System.NotImplementedException();
        }
    }
}