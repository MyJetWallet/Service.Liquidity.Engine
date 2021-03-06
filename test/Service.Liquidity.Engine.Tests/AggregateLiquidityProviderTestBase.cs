using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MyJetWallet.Domain.ExternalMarketApi.Models;
using NUnit.Framework;
using Service.AssetsDictionary.Domain.Models;
using Service.Balances.Domain.Models;
using Service.Liquidity.Engine.Domain.Models.Settings;
using Service.Liquidity.Engine.Domain.Models.Wallets;
using Service.Liquidity.Engine.Domain.Services.LiquidityProvider;
using Service.Liquidity.Engine.Domain.Services.MarketMakers;
using Service.Liquidity.Engine.Tests.Mock;

namespace Service.Liquidity.Engine.Tests
{
    public class AggregateLiquidityProviderTestBase
    {
        protected AggregateLiquidityProvider _engine;
        protected static ILoggerFactory _loggerFactory;

        protected OrderBookManagerMock _orderBookManager;
        protected MarketMakerSettingsAccessorMock _settingsMock;
        protected LpWalletManagerMock _walletManager;
        protected TradingServiceClientMock _tradingService;
        protected SpotInstrumentDictionaryMock _instrumentDictionaryMock;
        protected AssetsDictionaryMock _assetDictionary;
        protected ExternalBalanceCacheManagerMock _externalBalanceCacheManagerMock;

        [SetUp]
        public void Setup()
        {
            _orderBookManager = new();
            _settingsMock = new();
            _walletManager = new();
            _tradingService = new();
            _instrumentDictionaryMock = new();
            _assetDictionary = new AssetsDictionaryMock();
            _externalBalanceCacheManagerMock = new ExternalBalanceCacheManagerMock();

            _loggerFactory =
                LoggerFactory.Create(builder =>
                    builder.AddSimpleConsole(options =>
                    {
                        options.IncludeScopes = true;
                        options.SingleLine = true;
                        options.TimestampFormat = "hh:mm:ss ";
                    }));

            _engine = new AggregateLiquidityProvider(
                _loggerFactory.CreateLogger<AggregateLiquidityProvider>(),
                new OrderIdGenerator(),
                _orderBookManager,
                _settingsMock,
                _walletManager,
                _tradingService,
                _instrumentDictionaryMock,
                _assetDictionary,
                _externalBalanceCacheManagerMock
            );
        }

        protected void SetupEnvironment_1()
        {
            _settingsMock.MmSettings.Mode = EngineMode.Active;
            _settingsMock.MmSettings.UseExternalBalancePercentage = 100;

            _settingsMock.LpSettings.Add(new LiquidityProviderInstrumentSettings()
            {
                Mode = EngineMode.Active,
                ModeHedge = EngineMode.Active,
                LpWalletName = "LP-Wallet",
                Symbol = "BTCUSD",
                LpHedges = new List<LpHedgeSettings>(),
                LpSources = new List<LpSourceSettings>()
                {
                    new LpSourceSettings()
                    {
                        Mode = EngineMode.Active,
                        ExternalMarket = "FTX",
                        ExternalSymbol = "BTC/USD",
                        MaxBuySideVolume = 10,
                        MaxSellSideVolume = 10,
                        Markup = 0
                    }
                }
            });

            _walletManager.Wallets["LP-Wallet"] = new LpWallet()
            {
                BrokerId = "broker",
                ClientId = "client",
                WalletId = "ftx wallet",
                Name = "LP-Wallet"
            };

            _walletManager.Balances["LP-Wallet"] = new Dictionary<string, WalletBalance>()
            {
                {"BTC", new WalletBalance("BTC", 5, 0, DateTime.UtcNow, 1)},
                {"USD", new WalletBalance("USD", 300000, 0, DateTime.UtcNow, 1)}
            };

            _externalBalanceCacheManagerMock.Balances["FTX"] = new Dictionary<string, ExchangeBalance>();
            _externalBalanceCacheManagerMock.Balances["FTX"]["BTC"] = new ExchangeBalance()
            {
                Symbol = "BTC",
                Balance = 10,
                Free = 10
            };
            _externalBalanceCacheManagerMock.Balances["FTX"]["USD"] = new ExchangeBalance()
            {
                Symbol = "USD",
                Balance = 1000000,
                Free = 1000000
            };

            _externalBalanceCacheManagerMock.Markets["FTX"] = new Dictionary<string, ExchangeMarketInfo>();
            _externalBalanceCacheManagerMock.Markets["FTX"]["BTC/USD"] = new ExchangeMarketInfo()
            {
                Market = "BTC/USD",
                BaseAsset = "BTC",
                QuoteAsset = "USD",
                MinVolume = 0,
                PriceAccuracy = 0,
                VolumeAccuracy = 4
            };

            _orderBookManager.Data[("BTC/USD", "FTX")] = new LeOrderBook
            {
                Symbol = "BTC/USD",
                Source = "FTX",
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
                    new LeOrderBookLevel(60000, 2)
                }
            };

            _instrumentDictionaryMock.Data["BTCUSD"] = new SpotInstrument()
            {
                BrokerId = "broker",
                Symbol = "BTCUSD",
                BaseAsset = "BTC",
                QuoteAsset = "USD",
                IsEnabled = true,
                Accuracy = 2,
                MinVolume = 0.0001m,
                MaxVolume = 10m,
                MaxOppositeVolume = 1000000,
                KycRequiredForTrade = false
            };

            _assetDictionary.Data["BTC"] = new Asset()
            {
                BrokerId = "broker",
                Symbol = "BTC",
                IsEnabled = true,
                Accuracy = 4
            };

            _assetDictionary.Data["USD"] = new Asset()
            {
                BrokerId = "broker",
                Symbol = "USD",
                IsEnabled = true,
                Accuracy = 2
            };
        }

        [Test]
        public async Task Validate()
        {
            SetupEnvironment_1();

            _engine.Should().NotBeNull("engine should be inited");

            await _engine.RefreshOrders();

            _tradingService.CallList.Should().HaveCount(1);
            _tradingService.CallList.First().Orders.Should().HaveCount(10);
        }
    }
}
