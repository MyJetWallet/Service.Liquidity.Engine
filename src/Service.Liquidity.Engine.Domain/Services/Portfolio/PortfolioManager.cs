using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.Domain.Assets;
using MyJetWallet.Domain.Orders;
using MyJetWallet.Sdk.Service;
using Newtonsoft.Json;
using Service.AssetsDictionary.Client;
using Service.AssetsDictionary.Domain.Models;
using Service.Liquidity.Engine.Domain.Models.Portfolio;
using Service.Liquidity.Engine.Domain.Services.Wallets;
using Service.TradeHistory.ServiceBus;

namespace Service.Liquidity.Engine.Domain.Services.Portfolio
{
    public class PortfolioManager : IPortfolioManager, IStartable
    {
        private readonly ILogger<PortfolioManager> _logger;
        private readonly IPortfolioRepository _repository;
        private readonly ISpotInstrumentDictionaryClient _instrumentDictionary;
        private readonly IPortfolioReport _portfolioReport;
        private Dictionary<string, Dictionary<string, PositionPortfolio>> _data = new();
        private readonly object _sync = new();

        public PortfolioManager(
            ILogger<PortfolioManager> logger,
            IPortfolioRepository repository, 
            ISpotInstrumentDictionaryClient instrumentDictionary,
            IPortfolioReport portfolioReport)
        {
            _logger = logger;
            _repository = repository;
            _instrumentDictionary = instrumentDictionary;
            _portfolioReport = portfolioReport;
        }

        public async ValueTask RegisterLocalTrades(List<WalletTradeMessage> trades)
        {
            var toUpdate = new Dictionary<string, PositionPortfolio>();

            var baseId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var index = 1;

            foreach (var trade in trades)
            {
                var position = toUpdate.Values.FirstOrDefault(e => e.WalletId == trade.WalletId && e.Symbol == trade.Trade.InstrumentSymbol)
                               ?? GetPositionByWalletIdAndSymbol(trade.WalletId, trade.Trade.InstrumentSymbol);

                var action = "updated";
                if (position == null)
                {
                    position = CreateNewPosition($"{baseId}-{index++}", trade);
                    action = "created";
                    
                    if (position == null)
                        continue;
                }

                var reminder = position.ApplyTrade(trade.Trade.Side, (decimal)trade.Trade.Price, (decimal)trade.Trade.BaseVolume);
                await _portfolioReport.ReportPositionAssociation(new PositionAssociation(position.Id, trade.Trade.TradeUId, trade.WalletId, true));

                toUpdate[position.Id] = position;

                if (!position.IsOpen)
                {
                    await _portfolioReport.ReportClosePosition(position);
                }

                _logger.LogInformation("Register internal trade in portfolio: {jsonText}", JsonConvert.SerializeObject(trade));
                _logger.LogInformation("Position is {actionText}: {jsonText}", action, JsonConvert.SerializeObject(position));

                if (reminder != 0)
                {
                    var originalPosition = position;

                    position = CreateNewPosition($"{baseId}-{index++}", trade);
                    reminder = position.ApplyTrade(trade.Trade.Side, (decimal) trade.Trade.Price, reminder);
                    await _portfolioReport.ReportPositionAssociation(new PositionAssociation(position.Id, trade.Trade.TradeUId, trade.WalletId, true));

                    toUpdate[position.Id] = position;

                    if (reminder > 0)
                        _logger.LogError("After create reminder position, reminder still not zero. Trace: {josnText}",
                        JsonConvert.SerializeObject(new { reminder, originalPosition, position }));

                    _logger.LogInformation("Reminder Position is created: {jsonText}", JsonConvert.SerializeObject(position));
                }

                await _portfolioReport.ReportInternalTrade(CreateLocalTrade(trade));
            }

            if (toUpdate.Any())
            {
                using (var _ = MyTelemetry.StartActivity("Save position in repository")?.AddTag("position-count", toUpdate.Count))
                {
                    await _repository.Update(toUpdate.Values.ToList());
                }

                lock (_sync)
                {
                    foreach (var position in toUpdate.Values.Where(e => !e.IsOpen))
                    {
                        _data[position.WalletId].Remove(position.Symbol);
                    }

                    foreach (var position in toUpdate.Values.Where(e => e.IsOpen))
                    {
                        if (!_data.ContainsKey(position.WalletId))
                            _data[position.WalletId] = new Dictionary<string, PositionPortfolio>();
                        _data[position.WalletId][position.Symbol] = position;
                    }
                }
            }
        }

        private PortfolioTrade CreateLocalTrade(WalletTradeMessage trade)
        {
            var result = new PortfolioTrade(trade.Trade.TradeUId, trade.WalletId, true, trade.Trade.InstrumentSymbol,
                trade.Trade.Side, trade.Trade.Price,
                trade.Trade.BaseVolume, trade.Trade.QuoteVolume, trade.Trade.DateTime, string.Empty);

            return result;
        }

        public Task<List<PositionPortfolio>> GetPortfolio()
        {
            lock (_sync)
            {
                var result = _data.SelectMany(e => e.Value.Values).ToList();
                return Task.FromResult(result);
            }
        }

        private PositionPortfolio CreateNewPosition(string id, WalletTradeMessage trade)
        {
            var instrument = _instrumentDictionary.GetSpotInstrumentById(new SpotInstrumentIdentity()
            {
                BrokerId = trade.BrokerId, Symbol = trade.Trade.InstrumentSymbol
            });

            if (instrument == null)
            {
                _logger.LogError("Cannot found instrument to register trade: {jsonText}", JsonConvert.SerializeObject(trade));
                return null;
            }

            return new PositionPortfolio()
            {
                Id = id,
                Symbol = trade.Trade.InstrumentSymbol,
                BaseAsset = instrument.BaseAsset,
                QuotesAsset = instrument.QuoteAsset,
                IsOpen = true,
                OpenTime = DateTime.UtcNow,
                Side = trade.Trade.Side,
                WalletId = trade.WalletId
            };
        }

        public void Start()
        {
            var data = _repository.GetAll().GetAwaiter().GetResult();

            _data = data
                .GroupBy(e => e.WalletId)
                .ToDictionary(e => e.Key, e => e.ToDictionary(p => p.Symbol));
        }

        private PositionPortfolio GetPositionByWalletIdAndSymbol(string walletId, string symbol)
        {
            lock (_sync)
            {
                if (_data.TryGetValue(walletId, out var portfolio))
                    if (portfolio.TryGetValue(symbol, out var position))
                        return position;

                return null;
            }
        }
    }
}