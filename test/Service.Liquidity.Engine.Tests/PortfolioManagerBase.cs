using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Service.AssetsDictionary.Domain.Models;
using Service.Liquidity.Engine.Domain.Services.Portfolio;
using Service.Liquidity.Engine.Tests.Mock;

namespace Service.Liquidity.Engine.Tests
{
    public class PortfolioManagerBase
    {
        private static ILoggerFactory _loggerFactory;
        protected PortfolioManager _manager;
        protected PortfolioRepositoryMock _repository;
        protected SpotInstrumentDictionaryMock _instrumentDictionary;

        [SetUp]
        public void Setup()
        {
            _instrumentDictionary = new SpotInstrumentDictionaryMock();
            _instrumentDictionary.Data.Add("BTCUSD", new SpotInstrument()
            {
                Symbol = "BTCUSD",
                BaseAsset = "BTC",
                QuoteAsset = "USD"
            });

            _repository = new PortfolioRepositoryMock();


            _loggerFactory =
                LoggerFactory.Create(builder =>
                    builder.AddSimpleConsole(options =>
                    {
                        options.IncludeScopes = true;
                        options.SingleLine = true;
                        options.TimestampFormat = "hh:mm:ss ";
                    }));

            _manager = new PortfolioManager(
                _loggerFactory.CreateLogger<PortfolioManager>(),
                _repository,
                _instrumentDictionary
            );
        }
    }
}