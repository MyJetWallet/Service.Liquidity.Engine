using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using MyJetWallet.Domain.Orders;
using Service.Liquidity.Engine.Domain.Models.ExternalMarkets;
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

        public string GetName()
        {
            return Name;
        }

        public Task<double> GetBalance(string asset)
        {
            throw new System.NotImplementedException();
        }

        public Task<Dictionary<string, double>> GetBalances()
        {
            throw new System.NotImplementedException();
        }

        public Task<ExchangeMarketInfo> GetMarketInfo(string market)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<ExchangeMarketInfo>> GetMarketInfoListAsync()
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