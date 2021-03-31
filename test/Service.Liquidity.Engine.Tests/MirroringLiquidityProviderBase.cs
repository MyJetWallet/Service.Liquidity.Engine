using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Service.AssetsDictionary.Domain.Models;
using Service.Balances.Domain.Models;
using Service.Liquidity.Engine.Domain.Models;
using Service.Liquidity.Engine.Domain.Models.OrderBooks;
using Service.Liquidity.Engine.Domain.Models.Settings;
using Service.Liquidity.Engine.Domain.Models.Wallets;
using Service.Liquidity.Engine.Domain.Services.MarketMakers;
using Service.Liquidity.Engine.Tests.Mock;

namespace Service.Liquidity.Engine.Tests
{
    public class MirroringLiquidityProviderBase
    {
        protected MirroringLiquidityProvider _engine;
        protected ILoggerFactory _loggerFactory;

        protected OrderBookManagerMock _orderBookManager;
        protected MarketMakerSettingsAccessorMock _settingsMock;
        protected LpWalletManagerMock _walletManager;
        protected TradingServiceClientMock _tradingService;
        protected SpotInstrumentDictionaryMock _instrumentDictionaryMock;
        protected AssetsDictionaryMock _assetDictionary;

        [SetUp]
        public void Setup()
        {
            _orderBookManager = new();
            _settingsMock = new();
            _walletManager = new();
            _tradingService = new();
            _instrumentDictionaryMock = new();
            _assetDictionary = new AssetsDictionaryMock();

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
                _tradingService,
                _instrumentDictionaryMock,
                _assetDictionary
            );
        }

        protected void SetupEnvironment_1()
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
    }
}