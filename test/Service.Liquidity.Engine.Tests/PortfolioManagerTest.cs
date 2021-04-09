using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MyJetWallet.Domain.ExternalMarketApi.Models;
using MyJetWallet.Domain.Orders;
using Newtonsoft.Json;
using NUnit.Framework;
using Service.Liquidity.Engine.Domain.Models.Portfolio;
using Service.TradeHistory.Domain.Models;
using Service.TradeHistory.ServiceBus;

namespace Service.Liquidity.Engine.Tests
{
    public class PortfolioManagerTest : PortfolioManagerBase
    {
        [Test]
        public void Created()
        {
            Assert.NotNull(_manager);
        }


        [Test]
        public async Task LoadPositionOnStart()
        {
            _repository.Data["1"] = new PositionPortfolio()
            {
                Id = "1",
                WalletId = "TEST",
                Symbol = "BTCUSD",
                IsOpen = true
            };

            _repository.Data["2"] = new PositionPortfolio()
            {
                Id = "2",
                WalletId = "TEST",
                Symbol = "BTCEUR",
                IsOpen = true
            };

            _repository.Data["3"] = new PositionPortfolio()
            {
                Id = "3",
                WalletId = "WALLET",
                Symbol = "BTCUSD",
                IsOpen = true
            };

            _manager.Start();

            var positions = (await _manager.GetPortfolioAsync()).Where(e => e.WalletId == "TEST").ToList();
            positions.Select(e => e.Id).OrderBy(e => e).Should().BeEquivalentTo("1", "2");

            positions = (await _manager.GetPortfolioAsync()).Where(e => e.WalletId == "WALLET").ToList();
            positions.Select(e => e.Id).OrderBy(e => e).Should().BeEquivalentTo("3");
        }

        [Test]
        public async Task Test()
        {
            _manager.Start();

            List<WalletTradeMessage> trades = new List<WalletTradeMessage>(){
                new WalletTradeMessage()
                {
                    BrokerId = "broker",
                    WalletId = "TEST",
                    ClientId = "client",
                    Trade = new WalletTrade("1", "BTCUSD", 6000, 1, -6000, "1", OrderType.Market, 1, DateTime.UtcNow, OrderSide.Buy, 1),
                },

                new WalletTradeMessage()
                {
                    BrokerId = "broker",
                    WalletId = "TEST",
                    ClientId = "client",
                    Trade = new WalletTrade("2", "BTCUSD", 6000, 1, -6000, "2", OrderType.Market, 1, DateTime.UtcNow, OrderSide.Buy, 2),
                }
            };
            
            await _manager.RegisterLocalTradesAsync(trades);

            var positions1 = await _manager.GetPortfolioAsync();
            Assert.AreEqual(1, positions1.Count);
            Assert.AreEqual("BTCUSD", positions1[0].Symbol);
            Assert.AreEqual(OrderSide.Buy, positions1[0].Side);
            Assert.AreEqual(2, positions1[0].BaseVolume);
            Assert.AreEqual(-12000, positions1[0].QuoteVolume);

            Console.WriteLine($"Position 1: {JsonConvert.SerializeObject(positions1[0])}");

            trades = new List<WalletTradeMessage>(){
                new WalletTradeMessage()
                {
                    BrokerId = "broker",
                    WalletId = "TEST",
                    ClientId = "client",
                    Trade = new WalletTrade("3", "BTCUSD", 6000, -3, 6000, "3", OrderType.Market, 1, DateTime.UtcNow, OrderSide.Sell, 3),
                }
            };

            await _manager.RegisterLocalTradesAsync(trades);

            var positions2 = await _manager.GetPortfolioAsync();
            Assert.AreEqual(1, positions2.Count);
            Assert.AreEqual("BTCUSD", positions2[0].Symbol);
            Assert.AreEqual(OrderSide.Sell, positions2[0].Side);
            Assert.AreEqual(-1, positions2[0].BaseVolume);
            Assert.AreEqual(6000, positions2[0].QuoteVolume);

            Console.WriteLine($"Position 2: {JsonConvert.SerializeObject(positions2[0])}");

            Assert.AreEqual(1, _repository.Data.Values.Count);
            Assert.AreEqual(-1, _repository.Data.Values.First().BaseVolume);
            Assert.AreEqual(6000, _repository.Data.Values.First().QuoteVolume);


            trades = new List<WalletTradeMessage>(){
                new WalletTradeMessage()
                {
                    BrokerId = "broker",
                    WalletId = "TEST",
                    ClientId = "client",
                    Trade = new WalletTrade("4", "BTCUSD", 6000, 1, -6000, "4", OrderType.Market, 1, DateTime.UtcNow, OrderSide.Buy, 4),
                }
            };

            await _manager.RegisterLocalTradesAsync(trades);

            var positions3 = await _manager.GetPortfolioAsync();
            Assert.AreEqual(0, positions3.Count);

            Assert.AreEqual(0, _repository.Data.Values.Count);

        }

        [Test]
        public async Task ExternalTrade_PartialHedge()
        {
            _repository.Data["1"] = new PositionPortfolio()
            {
                Id = "1",
                WalletId = "TEST",
                Symbol = "BTCUSD",
                IsOpen = true,
                Side = OrderSide.Buy,
                BaseAsset = "BTC",
                QuotesAsset = "USD",
                BaseVolume = 2,
                QuoteVolume = -20000,
                OpenTime = DateTime.UtcNow
            };

            _manager.Start();

            var trade = new ExchangeTrade()
            {
                Id = "11",
                Price = 11000,
                Volume = -1,
                Side = OrderSide.Sell,
                Market = "BTC/USD",
                OppositeVolume = 11000,
                Timestamp = DateTime.UtcNow,
                ReferenceId = "1",

                Source = "FTX",
                AssociateSymbol = "BTCUSD",
                AssociateBrokerId = "broker",
                AssociateWalletId = "TEST"
            };

            await _manager.RegisterHedgeTradeAsync(trade);

            var position = _repository.Data["1"];

            position.Should().BeEquivalentTo(new 
            {
                IsOpen = true,
                Side = OrderSide.Buy,
                BaseAsset = "BTC",
                QuotesAsset = "USD",
                BaseVolume = 1,
                QuoteVolume = -9000
            });
        }

        [Test]
        public async Task ExternalTrade_FullHedge()
        {
            _repository.Data["1"] = new PositionPortfolio()
            {
                Id = "1",
                WalletId = "TEST",
                Symbol = "BTCUSD",
                IsOpen = true,
                Side = OrderSide.Buy,
                BaseAsset = "BTC",
                QuotesAsset = "USD",
                BaseVolume = 2,
                QuoteVolume = -20000,
                OpenTime = DateTime.UtcNow
            };

            _manager.Start();

            var trade = new ExchangeTrade()
            {
                Id = "11",
                Price = 11000,
                Volume = -2,
                Side = OrderSide.Sell,
                Market = "BTC/USD",
                OppositeVolume = 22000,
                Timestamp = DateTime.UtcNow,
                ReferenceId = "1",

                Source = "FTX",
                AssociateSymbol = "BTCUSD",
                AssociateBrokerId = "broker",
                AssociateWalletId = "TEST"
            };

            await _manager.RegisterHedgeTradeAsync(trade);

            _repository.Data.Should().BeEmpty();
            _portfolioReport.ClosedPosition.Should().HaveCount(1);

            var position = _portfolioReport.ClosedPosition.First();

            position.Should().BeEquivalentTo(new
            {
                IsOpen = false,
                Side = OrderSide.Buy,
                BaseAsset = "BTC",
                QuotesAsset = "USD",
                BaseVolume = 0,
                QuoteVolume = 2000
            });
            
        }

        [Test]
        public async Task ExternalTrade_OverHedge()
        {
            _repository.Data["1"] = new PositionPortfolio()
            {
                Id = "1",
                WalletId = "TEST",
                Symbol = "BTCUSD",
                IsOpen = true,
                Side = OrderSide.Sell,
                BaseAsset = "BTC",
                QuotesAsset = "USD",
                BaseVolume = -2,
                QuoteVolume = 20000,
                OpenTime = DateTime.UtcNow
            };

            _manager.Start();

            var trade = new ExchangeTrade()
            {
                Id = "11",
                Price = 11000,
                Volume = 3,
                Side = OrderSide.Buy,
                Market = "BTC/USD",
                OppositeVolume = -33000,
                Timestamp = DateTime.UtcNow,
                ReferenceId = "1",

                Source = "FTX",
                AssociateSymbol = "BTCUSD",
                AssociateBrokerId = "broker",
                AssociateWalletId = "TEST"
            };

            await _manager.RegisterHedgeTradeAsync(trade);

            _repository.Data.Should().HaveCount(1);
            _portfolioReport.ClosedPosition.Should().HaveCount(1);

            var position = _portfolioReport.ClosedPosition.First();

            position.Should().BeEquivalentTo(new
            {
                IsOpen = false,
                Side = OrderSide.Sell,
                BaseAsset = "BTC",
                QuotesAsset = "USD",
                BaseVolume = 0,
                QuoteVolume = -2000
            });

            position = _repository.Data.Values.First();

            position.Should().BeEquivalentTo(new
            {
                IsOpen = true,
                Side = OrderSide.Buy,
                BaseAsset = "BTC",
                QuotesAsset = "USD",
                BaseVolume = 1,
                QuoteVolume = -11000
            });
        }
    }
}