using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MyJetWallet.Domain.ExternalMarketApi.Models;
using NUnit.Framework;
using Service.Liquidity.Engine.Domain.Models.Settings;

namespace Service.Liquidity.Engine.Tests
{
    public class AggregateLiquidityProviderTest : AggregateLiquidityProviderTestBase
    {
        [Test]
        public async Task ValidateMaxSideVolume()
        {
            SetupEnvironment_1();

            _settingsMock.LpSettings.ForEach(e => e.LpSources.ForEach(s =>
            {
                s.MaxSellSideVolume = 1.1;
                s.MaxBuySideVolume = 1.2;
            }));

            await _engine.RefreshOrders();


            _tradingService.CallList.Should().HaveCount(1);

            _tradingService.CallList[0].Orders.Select(e => decimal.Parse(e.Volume)).Where(e => e < 0).Sum().Should().Be(-1.1m, "Sell side limited");
            _tradingService.CallList[0].Orders.Select(e => decimal.Parse(e.Volume)).Where(e => e > 0).Sum().Should().Be(1.2m, "Buy side limited");
        }

        [Test]
        public async Task FixNegativeSpread()
        {
            SetupEnvironment_1();

            SetupNegativeSpreadEnvironment();

            await _engine.RefreshOrders();


            _tradingService.CallList.Should().HaveCount(1);

            var orders = _tradingService.CallList.First().Orders.OrderByDescending(e => decimal.Parse(e.Price)).ToList();

            orders.Should().HaveCount(4);

            orders[0].Volume.Should().Be("-1");
            orders[1].Volume.Should().Be("-1");
            orders[2].Volume.Should().Be("1");
            orders[3].Volume.Should().Be("1");

            orders[0].Price.Should().Be("61200");
            orders[1].Price.Should().Be("60650.01");
            orders[2].Price.Should().Be("60649.99");
            orders[3].Price.Should().Be("60100");

            foreach (var order in orders)
            {
                Console.WriteLine($"{order.Price}  :  {order.Volume}");
            }
        }

        private void SetupNegativeSpreadEnvironment()
        {
            _orderBookManager.Data[("BTC/USD", "FTX-1")] = new LeOrderBook
            {
                Symbol = "BTC/USD",
                Source = "FTX-1",
                Timestamp = DateTime.UtcNow,
                Asks = new List<LeOrderBookLevel>()
                {
                    new LeOrderBookLevel(60200, 1)
                },
                Bids = new List<LeOrderBookLevel>()
                {
                    new LeOrderBookLevel(60100, 1)
                }
            };

            _orderBookManager.Data[("BTC/USD", "FTX-2")] = new LeOrderBook
            {
                Symbol = "BTC/USD",
                Source = "FTX-2",
                Timestamp = DateTime.UtcNow,
                Asks = new List<LeOrderBookLevel>()
                {
                    new LeOrderBookLevel(61200, 1)
                },
                Bids = new List<LeOrderBookLevel>()
                {
                    new LeOrderBookLevel(61100, 1)
                }
            };

            _settingsMock.LpSettings.First().LpSources = new List<LpSourceSettings>()
            {
                new LpSourceSettings()
                {
                    Mode = EngineMode.Active,
                    ExternalMarket = "FTX-1",
                    ExternalSymbol = "BTC/USD",
                    MaxBuySideVolume = 10,
                    MaxSellSideVolume = 10,
                    Markup = 0
                },
                new LpSourceSettings()
                {
                    Mode = EngineMode.Active,
                    ExternalMarket = "FTX-2",
                    ExternalSymbol = "BTC/USD",
                    MaxBuySideVolume = 10,
                    MaxSellSideVolume = 10,
                    Markup = 0
                }
            };

            _externalBalanceCacheManagerMock.Balances["FTX-1"] = new Dictionary<string, ExchangeBalance>();
            _externalBalanceCacheManagerMock.Balances["FTX-1"]["BTC"] = new ExchangeBalance()
            {
                Symbol = "BTC",
                Balance = 10,
                Free = 10
            };
            _externalBalanceCacheManagerMock.Balances["FTX-1"]["USD"] = new ExchangeBalance()
            {
                Symbol = "USD",
                Balance = 1000000,
                Free = 1000000
            };

            _externalBalanceCacheManagerMock.Markets["FTX-1"] = new Dictionary<string, ExchangeMarketInfo>();
            _externalBalanceCacheManagerMock.Markets["FTX-1"]["BTC/USD"] = new ExchangeMarketInfo()
            {
                Market = "BTC/USD",
                BaseAsset = "BTC",
                QuoteAsset = "USD",
                MinVolume = 0,
                PriceAccuracy = 0,
                VolumeAccuracy = 4
            };

            _externalBalanceCacheManagerMock.Balances["FTX-2"] = new Dictionary<string, ExchangeBalance>();
            _externalBalanceCacheManagerMock.Balances["FTX-2"]["BTC"] = new ExchangeBalance()
            {
                Symbol = "BTC",
                Balance = 10,
                Free = 10
            };
            _externalBalanceCacheManagerMock.Balances["FTX-2"]["USD"] = new ExchangeBalance()
            {
                Symbol = "USD",
                Balance = 1000000,
                Free = 1000000
            };

            _externalBalanceCacheManagerMock.Markets["FTX-2"] = new Dictionary<string, ExchangeMarketInfo>();
            _externalBalanceCacheManagerMock.Markets["FTX-2"]["BTC/USD"] = new ExchangeMarketInfo()
            {
                Market = "BTC/USD",
                BaseAsset = "BTC",
                QuoteAsset = "USD",
                MinVolume = 0,
                PriceAccuracy = 0,
                VolumeAccuracy = 4
            };
        }
    }
}