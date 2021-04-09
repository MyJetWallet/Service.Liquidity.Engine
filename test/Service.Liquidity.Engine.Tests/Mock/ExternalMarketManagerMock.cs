using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using MyJetWallet.Domain.ExternalMarketApi;
using MyJetWallet.Domain.ExternalMarketApi.Dto;
using MyJetWallet.Domain.ExternalMarketApi.Models;
using MyJetWallet.Domain.Orders;
using Service.Liquidity.Engine.Domain.Services.ExternalMarkets;

namespace Service.Liquidity.Engine.Tests.Mock
{
    public class ExternalMarketManagerMock: IExternalMarketManager
    {
        public Dictionary<string, List<ExchangeTrade>> Trades = new();
        public Dictionary<string, double> Prices = new();

        public IExternalMarket GetExternalMarketByName(string name)
        {
            lock(Trades)
            {
                if (!Trades.ContainsKey(name))
                {
                    throw new Exception($"Do not found Mock ExternalMarket: {name}");
                }

                return new ExternalMarketMock(name, Trades, Prices);
            }
        }

        public List<string> GetMarketNames()
        {
            throw new System.NotImplementedException();
        }
    }

    public class ExternalMarketMock : IExternalMarket
    {
        public Dictionary<string, List<ExchangeTrade>> Trades;
        public string Name { get; set; }
        public Dictionary<string, double> Prices;

        public ExternalMarketMock(string name, Dictionary<string, List<ExchangeTrade>> trades, Dictionary<string, double> prices)
        {
            Trades = trades;
            Prices = prices;
            Name = name;
        }


        public Task<GetNameResult> GetNameAsync()
        {
            return Task.FromResult(new GetNameResult() {Name = Name});
        }

        public Task<GetBalancesResponse> GetBalancesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<GetMarketInfoResponse> GetMarketInfoAsync(MarketRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetMarketInfoListResponse> GetMarketInfoListAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ExchangeTrade> MarketTrade(string market, OrderSide side, double volume, string referenceId)
        {
            if (!Prices.TryGetValue(market, out var price))
                throw new Exception($"Cannot found MOCK price for {market}");


            var oppVol = double.Parse(((decimal) price * (decimal) volume * -1m).ToString(CultureInfo.InvariantCulture));

            var trade = new ExchangeTrade()
            {
                Id = Guid.NewGuid().ToString("N"),
                Price = price,
                Side = side,
                Volume = volume,
                Market = market,
                OppositeVolume = oppVol,
                Timestamp = DateTime.UtcNow,
                ReferenceId = referenceId,
                Source = Name,
            };

            Trades[Name].Add(trade);

            return Task.FromResult(trade);
        }
    }
}