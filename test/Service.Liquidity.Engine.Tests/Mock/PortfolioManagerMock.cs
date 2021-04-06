﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Service.Liquidity.Engine.Domain.Models.ExternalMarkets;
using Service.Liquidity.Engine.Domain.Models.Portfolio;
using Service.Liquidity.Engine.Domain.Services.Portfolio;
using Service.TradeHistory.ServiceBus;

namespace Service.Liquidity.Engine.Tests.Mock
{
    public class PortfolioManagerMock: IPortfolioManager
    {
        public Dictionary<string, PositionPortfolio> Portfolio = new();
        public Dictionary<string, List<ExchangeTrade>> ExchangeTrades = new();


        public ValueTask RegisterLocalTradesAsync(List<WalletTradeMessage> trades)
        {
            return new ValueTask();
        }

        public Task RegisterHedgeTradeAsync(ExchangeTrade trade)
        {
            if (!ExchangeTrades.ContainsKey(trade.Source))
                ExchangeTrades[trade.Source] = new List<ExchangeTrade>();

            ExchangeTrades[trade.Source].Add(trade);
            return Task.CompletedTask;
        }

        public Task<List<PositionPortfolio>> GetPortfolioAsync()
        {
            return Task.FromResult(Portfolio.Values.ToList());
        }
    }
}