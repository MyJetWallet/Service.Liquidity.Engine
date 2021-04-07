using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Domain.Assets;
using MyJetWallet.Sdk.Service;
using Newtonsoft.Json;
using OpenTelemetry.Trace;
using Service.AssetsDictionary.Client;
using Service.Liquidity.Engine.Domain.Models.ExternalMarkets;
using Service.Liquidity.Engine.Domain.Models.Portfolio;
using Service.TradeHistory.ServiceBus;

namespace Service.Liquidity.Engine.Domain.Services.Portfolio
{
    public class PortfolioManager : IPortfolioManager
    {
        private readonly ILogger<PortfolioManager> _logger;
        private readonly IPortfolioRepository _repository;
        private readonly ISpotInstrumentDictionaryClient _instrumentDictionary;
        private readonly IPortfolioReport _portfolioReport;
        private Dictionary<string, Dictionary<string, PositionPortfolio>> _data = new();
        private readonly object _sync = new();

        private readonly MyAsyncLock _processLock = new MyAsyncLock();

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

        public async ValueTask RegisterLocalTradesAsync(List<WalletTradeMessage> trades)
        {
            using var lockProcess = await _processLock.Lock();

            var toUpdate = new Dictionary<string, PositionPortfolio>();

            var baseId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var index = 1;

            foreach (var trade in trades)
            {
                using var activity = MyTelemetry.StartActivity("Process local trade")
                    ?.AddTag("brokerId", trade.BrokerId)
                    .AddTag("clientId", trade.ClientId)
                    .AddTag("walletId", trade.WalletId)
                    .AddTag("tradeId", trade.Trade.TradeUId);

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

                activity?.AddTag("position-action", action);
                activity?.AddTag("positionId", position.Id);
                

                var reminder = await ApplyInternalTradeToPosition(position, trade);

                toUpdate[position.Id] = position;

                _logger.LogInformation("Register internal trade in portfolio: {jsonText}", JsonConvert.SerializeObject(trade));
                _logger.LogInformation("Position is {actionText}: {jsonText}", action, JsonConvert.SerializeObject(position));

                if (reminder != 0)
                {
                    using var activityReminder = MyTelemetry.StartActivity("Create position for reminder")
                        ?.AddTag("brokerId", trade.BrokerId)
                        .AddTag("clientId", trade.ClientId)
                        .AddTag("walletId", trade.WalletId)
                        .AddTag("tradeId", trade.Trade.TradeUId);

                    var originalPosition = position;

                    position = CreateNewPosition($"{baseId}-{index++}", trade);

                    reminder = await ApplyInternalTradeToPosition(position, trade);

                    toUpdate[position.Id] = position;

                    activity?.AddTag("position-action", "reminder");
                    activity?.AddTag("positionId", position.Id);

                    if (reminder > 0)
                    {
                        _logger.LogError("After create reminder position, reminder still not zero. Trace: {jsonText}",
                        JsonConvert.SerializeObject(new { reminder, originalPosition, position }));
                        activityReminder?.SetStatus(Status.Error);
                    }

                    _logger.LogInformation("Reminder Position is created: {jsonText}", JsonConvert.SerializeObject(position));
                }

                await _portfolioReport.ReportInternalTrade(CreateLocalTrade(trade));
            }

            if (toUpdate.Any())
            {
                using var _ = MyTelemetry.StartActivity("Save position updates")?.AddTag("position-count", toUpdate.Count);
                foreach (var position in toUpdate.Values)
                {
                    await UpdatePositionAndReport(position);
                }
            }
        }

        public async Task RegisterHedgeTradeAsync(ExchangeTrade trade)
        {
            using var lockProcess = await _processLock.Lock();

            using var activity = MyTelemetry.StartActivity("Process external trade")
                ?.AddTag("eternal-source", trade.Source)
                .AddTag("tradeId", trade.Id)
                .AddTag("symbol-external", trade.Market)
                .AddTag("symbol", trade.AssociateSymbol)
                .AddTag("walletId", trade.AssociateWalletId)
                .AddTag("brokerId", trade.AssociateBrokerId);

            var toUpdate = new List<PositionPortfolio>();

            var position = GetPositionByWalletIdAndSymbol(trade.AssociateWalletId, trade.AssociateSymbol);
            var reminderVolume = (decimal) trade.Volume;

            if (position != null)
            {
                activity?.AddTag("positionId", position.Id);

                reminderVolume = await ApplyExternalTradeToPosition(trade, position);

                toUpdate.Add(position);

                _logger.LogInformation("Register external trade in portfolio. Trade: {jsonText}", 
                    JsonConvert.SerializeObject(trade));
                
                _logger.LogInformation("Position is updated: {jsonText}", JsonConvert.SerializeObject(position));
            }

            if (reminderVolume != 0)
            {
                using var activityReminder = MyTelemetry.StartActivity("Create position for reminder")
                    ?.AddTag("eternal-source", trade.Source)
                    .AddTag("tradeId", trade.Id)
                    .AddTag("symbol-external", trade.Market)
                    .AddTag("symbol", trade.AssociateSymbol)
                    .AddTag("walletId", trade.AssociateWalletId);


                var originalPosition = position;

                var id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                position = CreateNewPosition($"{id}", trade);

                reminderVolume = await ApplyExternalTradeToPosition(trade, position);

                toUpdate.Add(position);

                activity?.AddTag("position-action", "reminder");
                activity?.AddTag("positionId", position.Id);

                if (reminderVolume > 0)
                {
                    _logger.LogError("After create reminder position, reminder still not zero. Trace: {jsonText}",
                        JsonConvert.SerializeObject(new { reminderVolume, originalPosition, position }));

                    activityReminder?.SetStatus(Status.Error);
                }

                _logger.LogInformation("Reminder Position is created: {jsonText}", JsonConvert.SerializeObject(position));
            }

            await _portfolioReport.ReportExternalTrade(CreateExternalTrade(trade));
            
            if (toUpdate.Any())
            {
                using var _ = MyTelemetry.StartActivity("Save position updates")?.AddTag("position-count", toUpdate.Count);
                foreach (var item in toUpdate)
                {
                    await UpdatePositionAndReport(item);
                }
            }
        }

        private async Task<decimal> ApplyExternalTradeToPosition(ExchangeTrade trade, PositionPortfolio position)
        {
            var reminderVolume = position.ApplyTrade(trade.Side, (decimal) trade.Price, (decimal) trade.Volume);

            await _portfolioReport.ReportPositionAssociation(new PositionAssociation(position.Id, trade.Id, trade.Source, false));

            return reminderVolume;
        }

        private async Task<decimal> ApplyInternalTradeToPosition(PositionPortfolio position, WalletTradeMessage trade)
        {
            var reminder = position.ApplyTrade(trade.Trade.Side, (decimal)trade.Trade.Price, (decimal)trade.Trade.BaseVolume);

            await _portfolioReport.ReportPositionAssociation(new PositionAssociation(position.Id, trade.Trade.TradeUId, trade.WalletId, true));

            return reminder;
        }

        private async Task UpdatePositionAndReport(PositionPortfolio position)
        {
            await _repository.Update(new List<PositionPortfolio>() { position });

            lock (_sync)
            {
                if (position.IsOpen)
                {
                    if (!_data.ContainsKey(position.WalletId))
                        _data[position.WalletId] = new Dictionary<string, PositionPortfolio>();
                    _data[position.WalletId][position.Symbol] = position;
                }
                else
                {
                    _data[position.WalletId].Remove(position.Symbol);
                }
            }

            await _portfolioReport.ReportPositionUpdate(position);
        }

        private PortfolioTrade CreateLocalTrade(WalletTradeMessage trade)
        {
            var result = new PortfolioTrade(trade.Trade.TradeUId, trade.WalletId, true, trade.Trade.InstrumentSymbol,
                trade.Trade.Side, trade.Trade.Price,
                trade.Trade.BaseVolume, trade.Trade.QuoteVolume, trade.Trade.DateTime, string.Empty);

            return result;
        }

        private PortfolioTrade CreateExternalTrade(ExchangeTrade trade)
        {
            var result = new PortfolioTrade(trade.Id, trade.Source, false, trade.Market,
                trade.Side, trade.Price,
                trade.Volume, trade.OppositeVolume, trade.Timestamp, trade.ReferenceId);

            return result;
        }

        public async Task<List<PositionPortfolio>> GetPortfolioAsync()
        {
            lock (_sync)
            {
                var result = _data.SelectMany(e => e.Value.Values).ToList();
                return result;
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

        private PositionPortfolio CreateNewPosition(string id, ExchangeTrade trade)
        {
            var instrument = _instrumentDictionary.GetSpotInstrumentById(new SpotInstrumentIdentity()
            {
                BrokerId = trade.AssociateBrokerId,
                Symbol = trade.AssociateSymbol
            });

            if (instrument == null)
            {
                _logger.LogError("Cannot found instrument to register trade: {jsonText}", JsonConvert.SerializeObject(trade));
                return null;
            }

            return new PositionPortfolio()
            {
                Id = id,
                Symbol = trade.AssociateSymbol,
                BaseAsset = instrument.BaseAsset,
                QuotesAsset = instrument.QuoteAsset,
                IsOpen = true,
                OpenTime = DateTime.UtcNow,
                Side = trade.Side,
                WalletId = trade.AssociateWalletId
            };
        }

        public void Start()
        {
            using var lockProcess = _processLock.Lock().Result;

            var data = _repository.GetAll().GetAwaiter().GetResult();


            lock (_sync)
            {
                _data = new Dictionary<string, Dictionary<string, PositionPortfolio>>();

                foreach (var walletPositions in data.GroupBy(e => e.WalletId))
                {
                    _data[walletPositions.Key] = new Dictionary<string, PositionPortfolio>();
                    foreach (var symbolPositions in walletPositions.GroupBy(e => e.Symbol))
                    {
                        if (symbolPositions.Count() == 1)
                        {
                            _data[walletPositions.Key][symbolPositions.Key] = symbolPositions.First();
                        }
                        else
                        {
                            _logger.LogError("Load many position by one instrument. Will skip all. Positions: {jsonText}", 
                                JsonConvert.SerializeObject(symbolPositions.ToList()));
                        }
                    }
                }
            }
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