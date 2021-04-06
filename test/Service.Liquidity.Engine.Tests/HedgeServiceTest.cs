using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MyJetWallet.Domain.Orders;
using NUnit.Framework;
using Service.Liquidity.Engine.Domain.Models.Portfolio;
using Service.Liquidity.Engine.Domain.Models.Settings;

namespace Service.Liquidity.Engine.Tests
{
    public class HedgeServiceTest : HedgeServiceTestBase
    {
        [Test]
        public void ServiceExist()
        {
            Assert.IsNotNull(_service);
        }

        [Test]
        public async Task DoNotHedge_GlobalSettingsModeDisable()
        {
            AddBuyPosition();

            _hedgeSettings.GlobalSettings.Mode = EngineMode.Disabled;

            await _service.HedgePortfolioAsync();

            _externalMarketManager.Trades["FTX"].Should().BeEmpty("Hedging is disabled, we should not trade");
        }

        [Test]
        public async Task DoNotHedge_GlobalSettingsModeIdle()
        {
            AddSellPosition();

            _hedgeSettings.GlobalSettings.Mode = EngineMode.Idle;

            await _service.HedgePortfolioAsync();

            _externalMarketManager.Trades["FTX"].Should().BeEmpty("Hedging is idle, we should not trade");
        }

        [Test]
        public async Task DoNotHedge_InstrumentSettingsModeDisable()
        {
            AddSellPosition();
            
            _hedgeSettings.GlobalSettings.Mode = EngineMode.Active;
            _hedgeSettings.InstrumentSettings["BTCUSD"].Mode = EngineMode.Disabled;

            await _service.HedgePortfolioAsync();

            _externalMarketManager.Trades["FTX"].Should().BeEmpty("Hedging is disabled, we should not trade");
        }

        [Test]
        public async Task DoNotHedge_InstrumentSettingsModeIdle()
        {
            AddBuyPosition();

            _hedgeSettings.GlobalSettings.Mode = EngineMode.Active;
            _hedgeSettings.InstrumentSettings["BTCUSD"].Mode = EngineMode.Idle;

            await _service.HedgePortfolioAsync();

            _externalMarketManager.Trades["FTX"].Should().BeEmpty("Hedging is idle, we should not trade");
        }

        [Test]
        public async Task Should_MakeTrade_If_PositionExist_Buy()
        {
            AddBuyPosition();

            _externalMarketManager.Prices["BTC/USD"] = 10000;

            await _service.HedgePortfolioAsync();

            _externalMarketManager.Trades["FTX"].Should().HaveCount(1, "Should make one trade");
            var trade = _externalMarketManager.Trades["FTX"].First();

            trade.Market.Should().Be("BTC/USD", "Wrong market");
            trade.Price.Should().Be(10000.0, "Wrong price");
            trade.Volume.Should().Be(-2, "Wrong Volume");
            trade.Side.Should().Be(OrderSide.Sell, "Wrong side");
            trade.OppositeVolume.Should().Be(20000.0, "Wrong opposite volume");
        }

        [Test]
        public async Task Should_MakeTrade_If_PositionExist_Sell()
        {
            AddSellPosition();

            _externalMarketManager.Prices["BTC/USD"] = 10000;

            await _service.HedgePortfolioAsync();

            _externalMarketManager.Trades["FTX"].Should().HaveCount(1, "Should make one trade");
            var trade = _externalMarketManager.Trades["FTX"].First();

            trade.Should().BeEquivalentTo(new
            {
                Market = "BTC/USD",
                Price = 10000.0,
                Volume = 2.0,
                Side = OrderSide.Buy,
                OppositeVolume = -20000.0,

                AssociateWalletId = "test-wallet",
                AssociateBrokerId = "broker",
                AssociateClientId = "client",
                AssociateSymbol = "BTCUSD"
            });
        }

        [Test]
        public async Task Should_RegisterTradeInPortfolio()
        {
            AddSellPosition();

            _externalMarketManager.Prices["BTC/USD"] = 10000;

            await _service.HedgePortfolioAsync();

            _externalMarketManager.Trades["FTX"].Should().HaveCount(1, "Should make one trade");
            var trade = _externalMarketManager.Trades["FTX"].First();

            _positioPortfolioManager.ExchangeTrades["FTX"].Should().HaveCount(1);

            _positioPortfolioManager.ExchangeTrades["FTX"].First().Should().BeEquivalentTo(trade);
        }







        private void AddBuyPosition()
        {
            _positioPortfolioManager.Portfolio["BTCUSD"] = new PositionPortfolio()
            {
                Id = Guid.NewGuid().ToString("N"),
                Symbol = "BTCUSD",
                BaseAsset = "BTC",
                QuotesAsset = "USD",
                BaseVolume = 2,
                QuoteVolume = -20000,
                IsOpen = true,
                OpenTime = DateTime.UtcNow,
                Side = OrderSide.Buy,
                WalletId = "test-wallet"
            };
        }

        private void AddSellPosition()
        {
            _positioPortfolioManager.Portfolio["BTCUSD"] = new PositionPortfolio()
            {
                Id = Guid.NewGuid().ToString("N"),
                Symbol = "BTCUSD",
                BaseAsset = "BTC",
                QuotesAsset = "USD",
                BaseVolume = -2,
                QuoteVolume = 20000,
                IsOpen = true,
                OpenTime = DateTime.UtcNow,
                Side = OrderSide.Sell,
                WalletId = "test-wallet"
            };
        }
    }
}