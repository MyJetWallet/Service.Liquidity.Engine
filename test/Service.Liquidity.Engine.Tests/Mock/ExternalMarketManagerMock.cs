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
        public Dictionary<string, ExchangeMarketInfo> MarketInfo = new();

        public IExternalMarket GetExternalMarketByName(string name)
        {
            lock(Trades)
            {
                if (!Trades.ContainsKey(name))
                {
                    throw new Exception($"Do not found Mock ExternalMarket: {name}");
                }

                return new ExternalMarketMock(name, Trades, Prices, MarketInfo);
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

        public Dictionary<string, ExchangeMarketInfo> MarketInfo { get; set; }

        public ExternalMarketMock(string name, Dictionary<string, List<ExchangeTrade>> trades, Dictionary<string, double> prices, Dictionary<string, ExchangeMarketInfo> marketInfo)
        {
            Trades = trades;
            Prices = prices;
            Name = name;
            MarketInfo = marketInfo;
        }


        public Task<GetNameResult> GetNameAsync(GetNameRequest request)
        {
            return Task.FromResult(new GetNameResult() {Name = Name});
        }

        public Task<GetBalancesResponse> GetBalancesAsync(GetBalancesRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetMarketInfoResponse> GetMarketInfoAsync(MarketRequest request)
        {
            if (MarketInfo.TryGetValue(request.Market, out var info))
                return Task.FromResult(new GetMarketInfoResponse(){Info = info });

            return Task.FromResult(new GetMarketInfoResponse() { Info = null });
        }

        public Task<GetMarketInfoListResponse> GetMarketInfoListAsync(GetMarketInfoListRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<ExchangeTrade> MarketTrade(MarketTradeRequest request)
        {
            if (!Prices.TryGetValue(request.Market, out var price))
                throw new Exception($"Cannot found MOCK price for {request.Market}");


            var oppVol = double.Parse(((decimal) price * (decimal)request.Volume * -1m).ToString(CultureInfo.InvariantCulture));

            var trade = new ExchangeTrade()
            {
                Id = Guid.NewGuid().ToString("N"),
                Price = price,
                Side = request.Side,
                Volume = request.Volume,
                Market = request.Market,
                OppositeVolume = oppVol,
                Timestamp = DateTime.UtcNow,
                ReferenceId = request.ReferenceId,
                Source = Name,
            };

            Trades[Name].Add(trade);

            return Task.FromResult(trade);
        }
    }
}