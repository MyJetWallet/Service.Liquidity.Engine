using System;
using FluentAssertions;
using MyJetWallet.Domain.Orders;
using NUnit.Framework;
using Service.AssetsDictionary.Domain.Models;
using Service.Liquidity.Engine.Domain.Models.Portfolio;

namespace Service.Liquidity.Engine.Tests
{
    public class PositionPortfolioTest
    {
        private PositionPortfolio _position;

        [SetUp]
        public void Setup()
        {
            _position = new PositionPortfolio()
            {
                Symbol = "BTCUSD",
                BaseAsset = "BTC",
                QuotesAsset = "USD",
                Id = "1",
                WalletId = "1",
                IsOpen = true,
                Side = OrderSide.Buy,
                OpenTime = DateTime.UtcNow,
            };
        }


        [Test]
        public void ApplyTrade_1()
        {
            _position.Side = OrderSide.Buy;

            var results = new decimal[4];
            results[0] = _position.ApplyTrade(OrderSide.Buy, 6000, 1);
            results[1] = _position.ApplyTrade(OrderSide.Buy, 6000, 1);
            results[2] = _position.ApplyTrade(OrderSide.Sell, 6050, -1);
            results[3] = _position.ApplyTrade(OrderSide.Sell, 6050, -1);

            results.Should().Equal(0m, 0m, 0m, 0m);

            Assert.AreEqual(0, _position.BaseVolume, "Wrong base volume");
            Assert.AreEqual(100, _position.QuoteVolume, "Wrong quote volume");
        }

        [Test]
        public void ApplyTrade_2()
        {
            _position.Side = OrderSide.Sell;

            var results = new decimal[4];
            results[0] = _position.ApplyTrade(OrderSide.Sell, 6000, -1);
            results[1] = _position.ApplyTrade(OrderSide.Sell, 6000, -1);
            results[2] = _position.ApplyTrade(OrderSide.Buy, 6050, 1);
            results[3] = _position.ApplyTrade(OrderSide.Buy, 6050, 1);

            results.Should().Equal(0m, 0m, 0m, 0m);

            Assert.AreEqual(0, _position.BaseVolume, "Wrong base volume");
            Assert.AreEqual(-100, _position.QuoteVolume, "Wrong quote volume");
            Assert.False(_position.IsOpen);
        }

        [Test]
        public void ApplyTrade_3()
        {
            _position.Side = OrderSide.Buy;

            var results = new decimal[2];
            results[0] = _position.ApplyTrade(OrderSide.Buy, 6000, 1);
            results[1] = _position.ApplyTrade(OrderSide.Sell, 6000, -1);

            results.Should().Equal(0m, 0m);

            Assert.AreEqual(0, _position.BaseVolume, "Wrong base volume");
            Assert.AreEqual(0, _position.QuoteVolume, "Wrong quote volume");
            Assert.False(_position.IsOpen);
        }

        [Test]
        public void ApplyTrade_4()
        {
            _position.Side = OrderSide.Buy;

            var results = new decimal[1];
            results[0] = _position.ApplyTrade(OrderSide.Sell, 6000, -1);

            results.Should().Equal(-1m);

            Assert.AreEqual(0, _position.BaseVolume, "Wrong base volume");
            Assert.AreEqual(0, _position.QuoteVolume, "Wrong quote volume");
            Assert.False(_position.IsOpen);
        }

        [Test]
        public void ApplyTrade_5()
        {
            _position.Side = OrderSide.Buy;

            var results = new decimal[2];
            results[0] = _position.ApplyTrade(OrderSide.Buy, 6000, 1);
            results[1] = _position.ApplyTrade(OrderSide.Sell, 6000, -2);

            results.Should().Equal(0m, -1m);

            Assert.AreEqual(0, _position.BaseVolume, "Wrong base volume");
            Assert.AreEqual(0, _position.QuoteVolume, "Wrong quote volume");
            Assert.False(_position.IsOpen);
        }

        [Test]
        public void ApplyTrade_6()
        {
            _position.Side = OrderSide.Sell;

            var results = new decimal[2];
            results[0] = _position.ApplyTrade(OrderSide.Sell, 6000, -1);
            results[1] = _position.ApplyTrade(OrderSide.Buy, 6000, 2);

            results.Should().Equal(0m, 1m);

            Assert.AreEqual(0, _position.BaseVolume, "Wrong base volume");
            Assert.AreEqual(0, _position.QuoteVolume, "Wrong quote volume");
            Assert.False(_position.IsOpen);
        }

        [Test]
        public void ApplyTrade_7()
        {
            _position.Side = OrderSide.Buy;

            var results = new decimal[1];
            results[0] = _position.ApplyTrade(OrderSide.Sell, 6000, -3);

            results.Should().Equal(-3m);

            Assert.AreEqual(0, _position.BaseVolume, "Wrong base volume");
            Assert.AreEqual(0, _position.QuoteVolume, "Wrong quote volume");
            Assert.False(_position.IsOpen);
        }

        [Test]
        public void ApplyTrade_8()
        {
            _position.Side = OrderSide.Sell;

            var results = new decimal[1];
            results[0] = _position.ApplyTrade(OrderSide.Buy, 6000, 3);

            results.Should().Equal(3m);

            Assert.AreEqual(0, _position.BaseVolume, "Wrong base volume");
            Assert.AreEqual(0, _position.QuoteVolume, "Wrong quote volume");
            Assert.False(_position.IsOpen);
        }

        [Test]
        public void ApplyTrade_9()
        {
            _position.Side = OrderSide.Sell;

            _position
                .Invoking(e => e.ApplyTrade(OrderSide.Buy, 6000, -3))
                .Should().Throw<Exception>();
        }

        [Test]
        public void ApplyClosePl_1()
        {
            _position.Side = OrderSide.Buy;
            _position.BaseVolume = 0;
            _position.QuoteVolume = 100;
            _position.IsOpen = true;

            _position.Invoking(p => p.ApplyClosePl(1.2m))
                .Should().Throw<Exception>();
        }

        [Test]
        public void ApplyClosePl_2()
        {
            _position.Side = OrderSide.Buy;
            _position.BaseVolume = 0;
            _position.QuoteVolume = 100;
            _position.IsOpen = false;

            _position.ApplyClosePl(1.2m);

            Assert.AreEqual(1.2m, _position.QuoteAssetToUsdPrice);
            Assert.AreEqual(120m, _position.PLUsd);
        }
    }
}