using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Service.Balances.Domain.Models;
using Service.Liquidity.Engine.Domain.Models;
using Service.Liquidity.Engine.Domain.Models.OrderBooks;
using Service.Liquidity.Engine.Domain.Models.Settings;
using Service.Liquidity.Engine.Domain.Models.Wallets;
using Service.Liquidity.Engine.Domain.Services.MarketMakers;

namespace Service.Liquidity.Engine.Tests
{
    public class MirroringLiquidityProviderTest
    {
        private MirroringLiquidityProvider _engine;
        private ILoggerFactory _loggerFactory;

        private OrderBookManagerMock _orderBookManager;
        private MarketMakerSettingsAccessorMock _settingsMock;
        private LpWalletManagerMock _walletManager;
        private TradingServiceClientMock _tradingService;

        [SetUp]
        public void Setup()
        {
            _orderBookManager = new();
            _settingsMock = new();
            _walletManager = new();
            _tradingService = new ();

            _loggerFactory =
                LoggerFactory.Create(builder =>
                    builder.AddSimpleConsole(options =>
                    {
                        options.IncludeScopes = true;
                        options.SingleLine = true;
                        options.TimestampFormat = "hh:mm:ss ";
                    }));

            _engine = new MirroringLiquidityProvider(
                _loggerFactory.CreateLogger<MirroringLiquidityProvider>(),
                new OrderIdGenerator(),
                _orderBookManager,
                _settingsMock,
                _walletManager,
                _tradingService
                );

        }

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

            Assert.AreEqual(60050m, request.Orders.Where(o => decimal.Parse(o.Volume) > 0).Min(e => decimal.Parse(e.Price)));
            Assert.AreEqual(60090m, request.Orders.Where(o => decimal.Parse(o.Volume) > 0).Max(e => decimal.Parse(e.Price)));
        }

        private void SetupEnvironment_1()
        {
            _settingsMock.MmSettings.Mode = EngineMode.Active;
            _settingsMock.MlSettings.Add(new MirroringLiquiditySettings()
            {
                Mode = EngineMode.Active,
                WalletName = "FTX",
                ExternalMarket = ExchangeNames.FTX,
                ExternalSymbol = "BTC/USD",
                InstrumentSymbol = "BTCUSD",
                Markup = 0
            });

            _walletManager.Wallets["FTX"] = new LpWallet()
            {
                BrokerId = "broker",
                ClientId = "client",
                WalletId = "ftx wallet",
                Name = "FTX"
            };

            _walletManager.Balances["FTX"] = new Dictionary<string, WalletBalance>()
            {
                {"BTC", new WalletBalance("BTC", 5, 0, DateTime.UtcNow, 1)},
                {"USD", new WalletBalance("USD", 300000, 0, DateTime.UtcNow, 1)}
            };

            _orderBookManager.Data[("BTC/USD", ExchangeNames.FTX)] = new LeOrderBook
            {
                Symbol = "BTC/USD",
                Source = ExchangeNames.FTX,
                Timestamp = DateTime.UtcNow,
                Asks = new List<LeOrderBookLevel>()
                {
                    new LeOrderBookLevel(60100, 0.4),
                    new LeOrderBookLevel(60110, 0.1),
                    new LeOrderBookLevel(60120, 0.5),
                    new LeOrderBookLevel(60130, 1),
                    new LeOrderBookLevel(60140, 2),
                },
                Bids = new List<LeOrderBookLevel>()
                {
                    new LeOrderBookLevel(60090, 0.4),
                    new LeOrderBookLevel(60080, 0.1),
                    new LeOrderBookLevel(60070, 0.5),
                    new LeOrderBookLevel(60060, 1),
                    new LeOrderBookLevel(60050, 2)
                }
            };
        }
    }
}