﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Service.Liquidity.Engine.Domain.Models;
using Service.Liquidity.Engine.Domain.Models.OrderBooks;
using Service.Liquidity.Engine.Domain.Models.Settings;

namespace Service.Liquidity.Engine.Tests
{
    public class MirroringLiquidityProviderTest: MirroringLiquidityProviderBase
    {

        [Test]
        public void Create_Engine()
        {
            Assert.NotNull(_engine);

            _loggerFactory.CreateLogger("Test").LogInformation("Debug output");

            Console.WriteLine("Debug output");
            Assert.Pass();
        }

        [Test]
        public async Task PlaceSameOrders_from_ExternalMarket()
        {
            SetupEnvironment_1();

            await _engine.RefreshOrders();

            var request = _tradingService.CallList.FirstOrDefault();

            Assert.IsNotNull(request);

            Assert.AreEqual(5, request.Orders.Count(o => decimal.Parse(o.Volume) < 0));
            Assert.AreEqual(5, request.Orders.Count(o => decimal.Parse(o.Volume) > 0));

            Assert.AreEqual(4m, request.Orders.Where(o => decimal.Parse(o.Volume) > 0).Sum(e => decimal.Parse(e.Volume)));
            Assert.AreEqual(-4m, request.Orders.Where(o => decimal.Parse(o.Volume) < 0).Sum(e => decimal.Parse(e.Volume)));

            Assert.AreEqual(60100m, request.Orders.Where(o => decimal.Parse(o.Volume) < 0).Min(e => decimal.Parse(e.Price)));
            Assert.AreEqual(60140m, request.Orders.Where(o => decimal.Parse(o.Volume) < 0).Max(e => decimal.Parse(e.Price)));

            Assert.AreEqual(60000m, request.Orders.Where(o => decimal.Parse(o.Volume) > 0).Min(e => decimal.Parse(e.Price)));
            Assert.AreEqual(60090m, request.Orders.Where(o => decimal.Parse(o.Volume) > 0).Max(e => decimal.Parse(e.Price)));

            var order = request.Orders.OrderByDescending(e => e.Price).First();
            Assert.AreEqual("-2", order.Volume);
            Assert.AreEqual("60140", order.Price);

            order = request.Orders.OrderBy(e => e.Price).First();
            Assert.AreEqual("2", order.Volume);
            Assert.AreEqual("60000", order.Price);
        }

        [Test]
        public async Task Check_MaxVolume()
        {
            SetupEnvironment_1();
            _instrumentDictionaryMock.Data.First().Value.MaxVolume = 1.5m;

            await _engine.RefreshOrders();

            var request = _tradingService.CallList.FirstOrDefault();

            Assert.IsNotNull(request);

            Assert.AreEqual(5, request.Orders.Count(o => decimal.Parse(o.Volume) < 0));
            Assert.AreEqual(5, request.Orders.Count(o => decimal.Parse(o.Volume) > 0));

            Assert.AreEqual(3.5m, request.Orders.Where(o => decimal.Parse(o.Volume) > 0).Sum(e => decimal.Parse(e.Volume)));
            Assert.AreEqual(-3.5m, request.Orders.Where(o => decimal.Parse(o.Volume) < 0).Sum(e => decimal.Parse(e.Volume)));

            Assert.AreEqual(60100m, request.Orders.Where(o => decimal.Parse(o.Volume) < 0).Min(e => decimal.Parse(e.Price)));
            Assert.AreEqual(60140m, request.Orders.Where(o => decimal.Parse(o.Volume) < 0).Max(e => decimal.Parse(e.Price)));

            Assert.AreEqual(60000m, request.Orders.Where(o => decimal.Parse(o.Volume) > 0).Min(e => decimal.Parse(e.Price)));
            Assert.AreEqual(60090m, request.Orders.Where(o => decimal.Parse(o.Volume) > 0).Max(e => decimal.Parse(e.Price)));

            var order = request.Orders.OrderByDescending(e => e.Price).First();
            Assert.AreEqual("-1.5", order.Volume);
            Assert.AreEqual("60140", order.Price);

            order = request.Orders.OrderBy(e => e.Price).First();
            Assert.AreEqual("1.5", order.Volume);
            Assert.AreEqual("60000", order.Price);
        }

        [Test]
        public async Task Check_MaxOppositeVolume()
        {
            SetupEnvironment_1();
            _instrumentDictionaryMock.Data.First().Value.MaxOppositeVolume = 90000;

            await _engine.RefreshOrders();

            var request = _tradingService.CallList.FirstOrDefault();

            Assert.IsNotNull(request);

            Assert.AreEqual(5, request.Orders.Count(o => decimal.Parse(o.Volume) < 0));
            Assert.AreEqual(5, request.Orders.Count(o => decimal.Parse(o.Volume) > 0));

            Assert.AreEqual(3.5m, request.Orders.Where(o => decimal.Parse(o.Volume) > 0).Sum(e => decimal.Parse(e.Volume)));
            Assert.AreEqual(-3.4965m, request.Orders.Where(o => decimal.Parse(o.Volume) < 0).Sum(e => decimal.Parse(e.Volume)));

            var order = request.Orders.OrderByDescending(e => e.Price).First();
            Assert.AreEqual("-1.4965", order.Volume);
            Assert.AreEqual("60140", order.Price);

            order = request.Orders.OrderBy(e => e.Price).First();
            Assert.AreEqual("1.5", order.Volume);
            Assert.AreEqual("60000", order.Price);
        }

        [Test]
        public async Task Check_MinVolume()
        {
            SetupEnvironment_1();
            _instrumentDictionaryMock.Data.First().Value.MinVolume = 1;

            await _engine.RefreshOrders();

            var request = _tradingService.CallList.FirstOrDefault();

            Assert.IsNotNull(request);

            Assert.AreEqual(2, request.Orders.Count(o => decimal.Parse(o.Volume) < 0));
            Assert.AreEqual(2, request.Orders.Count(o => decimal.Parse(o.Volume) > 0));

            Assert.AreEqual(3m, request.Orders.Where(o => decimal.Parse(o.Volume) > 0).Sum(e => decimal.Parse(e.Volume)));
            Assert.AreEqual(-3m, request.Orders.Where(o => decimal.Parse(o.Volume) < 0).Sum(e => decimal.Parse(e.Volume)));
        }

        [Test]
        public async Task Check_InstrumentIsDisable()
        {
            SetupEnvironment_1();
            _instrumentDictionaryMock.Data.First().Value.IsEnabled = false;

            await _engine.RefreshOrders();
            

            var request = _tradingService.CallList.FirstOrDefault();

            Assert.IsNull(request);
        }

        [Test]
        public async Task Check_SettingIsDisabled()
        {
            SetupEnvironment_1();
            _settingsMock.MlSettings.First().Mode = EngineMode.Disabled;

            await _engine.RefreshOrders();
            
            var request = _tradingService.CallList.FirstOrDefault();

            Assert.IsNotNull(request);

            Assert.AreEqual(0, request.Orders.Count);
        }

        [Test]
        public async Task Check_SettingIsIdle()
        {
            SetupEnvironment_1();
            _settingsMock.MlSettings.First().Mode = EngineMode.Idle;

            await _engine.RefreshOrders();

            var request = _tradingService.CallList.FirstOrDefault();

            Assert.IsNotNull(request);

            Assert.AreEqual(0, request.Orders.Count);
        }

        [Test]
        public async Task Check_GlobalSettingIsDisabled()
        {
            SetupEnvironment_1();
            _settingsMock.MmSettings.Mode = EngineMode.Disabled;

            await _engine.RefreshOrders();

            var request = _tradingService.CallList.FirstOrDefault();

            Assert.IsNotNull(request);

            Assert.AreEqual(0, request.Orders.Count);
        }

        [Test]
        public async Task Check_GlobalSettingIsIdle()
        {
            SetupEnvironment_1();
            _settingsMock.MmSettings.Mode = EngineMode.Idle;

            await _engine.RefreshOrders();

            var request = _tradingService.CallList.FirstOrDefault();

            Assert.IsNotNull(request);

            Assert.AreEqual(0, request.Orders.Count);
        }

        [Test]
        public async Task Check_Balance_SellSide()
        {
            SetupEnvironment_1();
            _walletManager.Balances["FTX"]["BTC"].Balance = 0.7;

            await _engine.RefreshOrders();


            var request = _tradingService.CallList.FirstOrDefault();

            Assert.IsNotNull(request);

            Assert.AreEqual(3, request.Orders.Count(o => decimal.Parse(o.Volume) < 0));
            Assert.AreEqual(5, request.Orders.Count(o => decimal.Parse(o.Volume) > 0));

            Assert.AreEqual(4m, request.Orders.Where(o => decimal.Parse(o.Volume) > 0).Sum(e => decimal.Parse(e.Volume)));
            Assert.AreEqual(-0.7m, request.Orders.Where(o => decimal.Parse(o.Volume) < 0).Sum(e => decimal.Parse(e.Volume)));
        }

        [Test]
        public async Task Check_Balance_SellSide_RoundDownVolume()
        {
            SetupEnvironment_1();
            _assetDictionary.Data["BTC"].Accuracy = 2;

            _orderBookManager.Data[("BTC/USD", ExchangeNames.FTX)] = new LeOrderBook
            {
                Symbol = "BTC/USD",
                Source = ExchangeNames.FTX,
                Timestamp = DateTime.UtcNow,
                Asks = new List<LeOrderBookLevel>()
                {
                    new LeOrderBookLevel(60092, 0.9999),
                },
                Bids = new List<LeOrderBookLevel>()
                
            };

            await _engine.RefreshOrders();


            var request = _tradingService.CallList.FirstOrDefault();

            Assert.IsNotNull(request);

            Assert.AreEqual(1, request.Orders.Count);

            Assert.AreEqual("-0.99", request.Orders.First().Volume);
        }

        [Test]
        public async Task Check_Balance_SellSide_RoundUpPrice()
        {
            SetupEnvironment_1();
            _instrumentDictionaryMock.Data.First().Value.Accuracy = 2;

            _orderBookManager.Data[("BTC/USD", ExchangeNames.FTX)] = new LeOrderBook
            {
                Symbol = "BTC/USD",
                Source = ExchangeNames.FTX,
                Timestamp = DateTime.UtcNow,
                Asks = new List<LeOrderBookLevel>()
                {
                    new LeOrderBookLevel(60092.11111, 1),
                },
                Bids = new List<LeOrderBookLevel>()

            };

            await _engine.RefreshOrders();


            var request = _tradingService.CallList.FirstOrDefault();

            Assert.IsNotNull(request);

            Assert.AreEqual(1, request.Orders.Count);

            Assert.AreEqual("60092.12", request.Orders.First().Price);
        }

        [Test]
        public async Task Check_Balance_BuySide()
        {
            SetupEnvironment_1();
            _walletManager.Balances["FTX"]["USD"].Balance = 42058;

            await _engine.RefreshOrders();


            var request = _tradingService.CallList.FirstOrDefault();

            Assert.IsNotNull(request);

            Assert.AreEqual(5, request.Orders.Count(o => decimal.Parse(o.Volume) < 0));
            Assert.AreEqual(3, request.Orders.Count(o => decimal.Parse(o.Volume) > 0));

            Assert.AreEqual(0.7m, request.Orders.Where(o => decimal.Parse(o.Volume) > 0).Sum(e => decimal.Parse(e.Volume)));
            Assert.AreEqual(-4m, request.Orders.Where(o => decimal.Parse(o.Volume) < 0).Sum(e => decimal.Parse(e.Volume)));
        }

        [Test]
        public async Task Check_Balance_BuySide_RoundDownVolume()
        {
            SetupEnvironment_1();
            _assetDictionary.Data["BTC"].Accuracy = 4;

            _walletManager.Balances["FTX"]["USD"].Balance = 40057.32;

            _orderBookManager.Data[("BTC/USD", ExchangeNames.FTX)] = new LeOrderBook
            {
                Symbol = "BTC/USD",
                Source = ExchangeNames.FTX,
                Timestamp = DateTime.UtcNow,
                Asks = new List<LeOrderBookLevel>(),
                Bids = new List<LeOrderBookLevel>()
                {
                    new LeOrderBookLevel(60092, 0.6666),
                }
            };


            await _engine.RefreshOrders();


            var request = _tradingService.CallList.FirstOrDefault();

            Assert.IsNotNull(request);

            Assert.AreEqual(0, request.Orders.Count(o => decimal.Parse(o.Volume) < 0));
            Assert.AreEqual(1, request.Orders.Count(o => decimal.Parse(o.Volume) > 0));

            Assert.AreEqual("0.6665", request.Orders.First().Volume);
        }

        [Test]
        public async Task Check_Balance_BuySide_RoundDownPrice()
        {
            SetupEnvironment_1();
            _instrumentDictionaryMock.Data.First().Value.Accuracy = 2;

            _orderBookManager.Data[("BTC/USD", ExchangeNames.FTX)] = new LeOrderBook
            {
                Symbol = "BTC/USD",
                Source = ExchangeNames.FTX,
                Timestamp = DateTime.UtcNow,
                Asks = new List<LeOrderBookLevel>(),
                Bids = new List<LeOrderBookLevel>()
                {
                    new LeOrderBookLevel(60092.9999, 1),
                }
            };


            await _engine.RefreshOrders();


            var request = _tradingService.CallList.FirstOrDefault();

            Assert.IsNotNull(request);

            Assert.AreEqual(1, request.Orders.Count);

            Assert.AreEqual("60092.99", request.Orders.First().Price);
        }

        [Test]
        public async Task Apply_Markup()
        {
            SetupEnvironment_1();
            _settingsMock.MlSettings.First().Markup = 0.1;

            await _engine.RefreshOrders();


            var request = _tradingService.CallList.FirstOrDefault();

            Assert.IsNotNull(request);

            var orders = request.Orders.OrderBy(e => e.Price).ToArray();

            Assert.AreEqual("54000", orders[0].Price, "wrong price 1");
            Assert.AreEqual("54054", orders[1].Price, "wrong price 2");
            Assert.AreEqual("54063", orders[2].Price, "wrong price 3");
            Assert.AreEqual("54072", orders[3].Price, "wrong price 4");
            Assert.AreEqual("54081", orders[4].Price, "wrong price 5");
            
            Assert.AreEqual("66110", orders[5].Price, "wrong price 6");
            Assert.AreEqual("66121", orders[6].Price, "wrong price 7");
            Assert.AreEqual("66132", orders[7].Price, "wrong price 8");
            Assert.AreEqual("66143", orders[8].Price, "wrong price 9");
            Assert.AreEqual("66154", orders[9].Price, "wrong price 10");
        }

        [Test]
        public async Task Check_MaxSellVolume()
        {
            SetupEnvironment_1();
            _settingsMock.MlSettings.First().MaxSellSideVolume = 0.7;
            _settingsMock.MlSettings.First().MaxBuySideVolume = 0.7;

            await _engine.RefreshOrders();

            var request = _tradingService.CallList.FirstOrDefault();

            Assert.IsNotNull(request);

            Assert.AreEqual(3, request.Orders.Count(o => decimal.Parse(o.Volume) < 0));
            Assert.AreEqual(3, request.Orders.Count(o => decimal.Parse(o.Volume) > 0));

            var orders = request.Orders.OrderBy(e => e.Price).ToArray();

            Assert.AreEqual("0.2", orders[0].Volume, "wrong price 1");
            Assert.AreEqual("0.1", orders[1].Volume, "wrong price 2");
            Assert.AreEqual("0.4", orders[2].Volume, "wrong price 3");
            Assert.AreEqual("-0.4", orders[3].Volume, "wrong price 4");
            Assert.AreEqual("-0.1", orders[4].Volume, "wrong price 5");
            Assert.AreEqual("-0.2", orders[5].Volume, "wrong price 6");
        }


    }


}