using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Service.Liquidity.Engine.Domain.Models.ExternalMarkets;
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



            _service = new HedgeService(
                _loggerFactory.CreateLogger<HedgeService>(),
                _positioPortfolioManager,
                _hedgeSettings,
                _hedgeSettings,
                _externalMarketManager,
                _walletManager);


            _hedgeSettings.GlobalSettings.Mode = EngineMode.Active;

            _hedgeSettings.InstrumentSettings["BTCUSD"] = new HedgeInstrumentSettings()
            {
                WalletId = "test-wallet",
                InstrumentSymbol = "BTCUSD",
                ExternalMarket = "FTX",
                ExternalSymbol = "BTC/USD",
                MinVolume = 0.01,
                Mode = EngineMode.Active
            };

            _externalMarketManager.Prices["BTC/USD"] = 10000;
            _externalMarketManager.Trades["FTX"] = new List<ExchangeTrade>();

            _walletManager.Wallets["test-wallet"] = new LpWallet()
            {
                WalletId = "test-wallet",
                BrokerId = "broker",
                ClientId = "client",
                Name = "NAME"
            };

        }
    }
}