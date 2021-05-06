using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MyJetWallet.Domain.ExternalMarketApi.Models;
using NUnit.Framework;
using Service.Liquidity.Engine.Domain.Models.Settings;
using Service.Liquidity.Engine.Domain.Models.Wallets;
using Service.Liquidity.Engine.Domain.Services.Hedger;
using Service.Liquidity.Engine.Tests.Mock;

namespace Service.Liquidity.Engine.Tests
{
    public class HedgeServiceTestBase
    {
        protected HedgeService _service;
        private ILoggerFactory _loggerFactory;
        protected PortfolioManagerMock _positioPortfolioManager;
        protected HedgeSettingsManagerMock _hedgeSettings;
        protected ExternalMarketManagerMock _externalMarketManager;
        protected LpWalletManagerMock _walletManager;
        protected MarketMakerSettingsAccessorMock _settingsMock;
        protected OrderBookManagerMock _orderBookManager;
        protected ExternalBalanceCacheManagerMock _externalBalanceCacheManagerMock;

        [SetUp]
        public void Setup()
        {
            _loggerFactory =
                LoggerFactory.Create(builder =>
                    builder.AddSimpleConsole(options =>
                    {
                        options.IncludeScopes = true;
                        options.SingleLine = true;
                        options.TimestampFormat = "hh:mm:ss ";
                    }));

            _positioPortfolioManager = new PortfolioManagerMock();
            _hedgeSettings = new HedgeSettingsManagerMock();
            _externalMarketManager = new ExternalMarketManagerMock();
            _walletManager = new LpWalletManagerMock();
            _settingsMock = new();
            _orderBookManager = new OrderBookManagerMock();
            _externalBalanceCacheManagerMock = new ExternalBalanceCacheManagerMock();


            _service = new HedgeService(
                _loggerFactory.CreateLogger<HedgeService>(),
                _positioPortfolioManager,
                _hedgeSettings,
                _externalMarketManager,
                _walletManager,
                _settingsMock,
                _orderBookManager,
                _externalBalanceCacheManagerMock
                );


            _hedgeSettings.GlobalSettings.Mode = EngineMode.Active;


            _settingsMock.MmSettings.Mode = EngineMode.Disabled;
            _settingsMock.MmSettings.UseExternalBalancePercentage = 100;

            _settingsMock.LpSettings.Add(new LiquidityProviderInstrumentSettings()
            {
                Mode = EngineMode.Active,
                ModeHedge = EngineMode.Active,
                LpWalletName = "NAME",
                Symbol = "BTCUSD",
                LpSources = new List<LpSourceSettings>(),
                LpHedges = new List<LpHedgeSettings>()
                {
                    new LpHedgeSettings()
                    {
                        Mode = EngineMode.Active,
                        ExternalMarket = "FTX",
                        ExternalSymbol = "BTC/USD",
                        MinVolume = 0.01
                    }
                },
                WalletId = "LP-Wallet",
                BrokerId = "broker"
            });

            _externalMarketManager.Prices["BTC/USD"] = 10000;
            _externalMarketManager.Trades["FTX"] = new List<ExchangeTrade>();
            _externalMarketManager.MarketInfo["BTC/USD"] = new ExchangeMarketInfo()
            {
                Market = "BTC/USD",
                BaseAsset = "BTC",
                QuoteAsset = "USD",
                MinVolume = 0.01,
                VolumeAccuracy = 4,
                PriceAccuracy = 2
            };

            _walletManager.Wallets["NAME"] = new LpWallet()
            {
                WalletId = "LP-Wallet",
                BrokerId = "broker",
                ClientId = "client",
                Name = "NAME"
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
        }

        [Test]
        public async Task Validate()
        {
            _service.Should().NotBeNull();

            await _service.HedgePortfolioAsync();
        }
    }
}