using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Domain.ExternalMarketApi;
using MyJetWallet.Domain.ExternalMarketApi.Dto;
using MyJetWallet.Domain.ExternalMarketApi.Models;
using MyJetWallet.Domain.Orders;
using MyJetWallet.Sdk.Service;
using Newtonsoft.Json;
using OpenTelemetry.Trace;
using Service.AssetsDictionary.Domain.Models;
using Service.Liquidity.Engine.Domain.Models.LiquidityProvider;
using Service.Liquidity.Engine.Domain.Models.Portfolio;
using Service.Liquidity.Engine.Domain.Models.Settings;
using Service.Liquidity.Engine.Domain.Services.ExternalMarkets;
using Service.Liquidity.Engine.Domain.Services.MarketMakers;
using Service.Liquidity.Engine.Domain.Services.OrderBooks;
using Service.Liquidity.Engine.Domain.Services.Portfolio;
using Service.Liquidity.Engine.Domain.Services.Settings;
using Service.Liquidity.Engine.Domain.Services.Wallets;

namespace Service.Liquidity.Engine.Domain.Services.Hedger
{
    public class HedgeService : IHedgeService
    {
        private readonly ILogger<HedgeService> _logger;
        private readonly IPortfolioManager _portfolioManager;
        private readonly IHedgeSettingsManager _settingsManager;
        private readonly IExternalMarketManager _externalMarketManager;
        private readonly ILpWalletManager _lpWalletManager;
        private readonly IMarketMakerSettingsAccessor _settingsAccessor;
        private readonly IOrderBookManager _orderBookManager;
        private readonly IExternalBalanceCacheManager _externalBalanceCacheManager;

        private readonly Dictionary<string, (DateTime, string)> _skippedSources = new();

        public HedgeService(
            ILogger<HedgeService> logger,
            IPortfolioManager portfolioManager,
            IHedgeSettingsManager settingsManager,
            IExternalMarketManager externalMarketManager,
            ILpWalletManager lpWalletManager,
            IMarketMakerSettingsAccessor settingsAccessor,
            IOrderBookManager orderBookManager,
            IExternalBalanceCacheManager externalBalanceCacheManager
            )
        {
            _logger = logger;
            _portfolioManager = portfolioManager;
            _settingsManager = settingsManager;
            _externalMarketManager = externalMarketManager;
            _lpWalletManager = lpWalletManager;
            _settingsAccessor = settingsAccessor;
            _orderBookManager = orderBookManager;
            _externalBalanceCacheManager = externalBalanceCacheManager;
        }

        public async Task HedgePortfolioAsync()
        {
            using var _ = MyTelemetry.StartActivity("Hedge portfolio");

            var portfolio = await _portfolioManager.GetPortfolioAsync();

            var globalSettings = _settingsAccessor.GetMarketMakerSettings();
            var globalHedgeSettings = _settingsManager.GetGlobalHedgeSettings();
            var instrumentSettings = _settingsAccessor.GetLiquidityProviderSettings();

            ValidateSkipSourceList(globalHedgeSettings.SkipSourceTimeoutSec);

            foreach (var positionPortfolio in portfolio)
            {
                await HedgePositionAsync(positionPortfolio, globalHedgeSettings, instrumentSettings, globalSettings);
            }
        }

        private void ValidateSkipSourceList(int skipSourceTimeoutSec)
        {
            lock (_skippedSources)
            {
                foreach (var source in _skippedSources.ToList())
                {
                    if ((DateTime.UtcNow - source.Value.Item1).TotalSeconds > skipSourceTimeoutSec)
                    {
                        _skippedSources.Remove(source.Key);
                        _logger.LogInformation("External source '{source}' exclude from skip list by timeout", source.Key);
                    }
                }
            }
        }

        

        private async Task HedgePositionAsync(
            PositionPortfolio positionPortfolio, 
            HedgeSettings globalHedgeSettings, //todo: remove it and add to MarketMakerSettings
            List<LiquidityProviderInstrumentSettings> instrumentSettingsList, 
            MarketMakerSettings marketMakerSettings)
        {
            using var activity = MyTelemetry.StartActivity("Hedge portfolio position");

            activity?.AddTag("positionId", positionPortfolio.Id)
                .AddTag("symbol", positionPortfolio.Symbol);



            var instrumentSettings = instrumentSettingsList.FirstOrDefault(e => 
                e.Symbol == positionPortfolio.Symbol &&
                e.WalletId == positionPortfolio.WalletId); //todo: validate brokerId

            if (globalHedgeSettings == null || instrumentSettings == null)
            {
                activity?.AddTag("hedge-mode", "no-settings");
                return;
            }

            try
            {
                if (globalHedgeSettings.Mode != EngineMode.Active)
                {
                    activity?.AddTag("hedge-mode", globalHedgeSettings.Mode.ToString());
                    activity?.AddTag("hedge-message", "global disabled");
                    return;
                }

                if (instrumentSettings.Mode != EngineMode.Active)
                {
                    activity?.AddTag("hedge-mode", instrumentSettings.Mode.ToString());
                    activity?.AddTag("hedge-message", "instrument disabled");
                    return;
                }

                var wallet = _lpWalletManager.GetWallet(instrumentSettings.LpWalletName);
                if (wallet == null)
                {
                    _logger.LogError("Cannot hedge position {positionId}, because wallet '{marketName}' not found",
                        positionPortfolio.Id, instrumentSettings.LpWalletName);
                    activity?.AddTag("hedge-message", "lp wallet not found");
                    activity?.SetStatus(Status.Error);
                    return;
                }

                activity?.AddTag("walletName", wallet.Name);

                var externalMarkets = await GetAvailableExternalMarkets(positionPortfolio, instrumentSettings);

                if (!externalMarkets.Any())
                {
                    _logger.LogWarning("Cannot hedge position {positionId}, not found external markets", positionPortfolio.Id, instrumentSettings.LpWalletName);
                    activity?.AddTag("hedge-message", "external markets not found");
                    activity?.SetStatus(Status.Error);
                    return;
                }


                List<LpOrder> externalOrders = new();

                var remainingVolume = (double)Math.Abs(positionPortfolio.BaseVolume);

                foreach (var externalMarketItem in externalMarkets.Where(e => remainingVolume >= e.Value.Item3.MinVolume))
                {
                    var orders = await GenerateOrdersFromSourceAsync(
                        externalMarketItem.Value.Item1, externalMarketItem.Value.Item2, externalMarketItem.Key, marketMakerSettings.UseExternalBalancePercentage, positionPortfolio.Side);
                    externalOrders.AddRange(orders);
                }

                if (!externalOrders.Any())
                {
                    activity?.AddTag("hedge-message", "do not found available option to trade");
                    return;
                }

                List<LpOrder> externalOrdersToExecute = new();

                var sortedOrders = positionPortfolio.Side == OrderSide.Buy
                    ? externalOrders.OrderBy(e => e.Price)
                    : externalOrders.OrderByDescending(e => e.Price);
                
                foreach (var lpOrder in sortedOrders)
                {
                    if (lpOrder.Volume >= remainingVolume)
                    {
                        var info = externalMarkets[lpOrder.Source].Item2;

                        if (remainingVolume >= info.MinVolume)
                        {
                            lpOrder.Volume = Math.Round(remainingVolume, info.VolumeAccuracy, MidpointRounding.ToZero);
                            externalOrdersToExecute.Add(lpOrder);
                        }
                        break;
                    }

                    remainingVolume -= lpOrder.Volume;
                    externalOrdersToExecute.Add(lpOrder);
                }

                var iteration = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                foreach (var source in externalOrdersToExecute.GroupBy(e => e.Source))
                {
                    var market = externalMarkets[source.Key].Item1;
                    var info = externalMarkets[source.Key].Item2;

                    var side = positionPortfolio.Side == OrderSide.Buy ? OrderSide.Sell : OrderSide.Buy;

                    var volume = source.Sum(e => e.Volume);
                    if (side == OrderSide.Sell)
                        volume = -volume;
                    volume = Math.Round(volume, info.VolumeAccuracy, MidpointRounding.ToZero);

                    var tradeRequest = new MarketTradeRequest()
                    {
                        ReferenceId = $"{positionPortfolio.Id}__{iteration}",
                        Side = positionPortfolio.Side == OrderSide.Buy ? OrderSide.Sell : OrderSide.Buy,
                        Volume = volume,
                        Market = info.Market
                    };

                    _logger.LogInformation("Try to execute external trade. PositionId {positionId}; TradeRequest: {tradeJson}", positionPortfolio.Id, JsonConvert.SerializeObject(tradeRequest));

                    ExchangeTrade trade;
                    try
                    {
                        trade = await market.MarketTrade(tradeRequest);

                        if (trade == null)
                        {
                            throw new Exception("Receive empty trade from external market");
                        }
                    }
                    catch(Exception ex)
                    {
                        SkipSource(source.Key, $"Error on make trade: {ex.ToString()}");
                        _logger.LogError(ex, "Cannot hedge position on external market: {sourceText}. PositionId: {positionId}; ExternalMarket: {externalMarketName};  TradeRequest: {tradeJson}",
                            source.Key, 
                            positionPortfolio.Id,

                            JsonConvert.SerializeObject(tradeRequest));

                        return;
                    }

                    trade.AssociateSymbol = positionPortfolio.Symbol;
                    trade.AssociateBrokerId = wallet.BrokerId;
                    trade.AssociateClientId = wallet.ClientId;
                    trade.AssociateWalletId = wallet.WalletId;

                    trade.AddToActivityAsJsonTag("external-trade-result");

                    _logger.LogInformation("Executed hedge trade. PositionId {positionId}. Trade: {tradeJson}",
                        positionPortfolio.Id,
                        JsonConvert.SerializeObject(trade));

                    activity?.AddTag("external-trade-id", trade.Id);


                    await _portfolioManager.RegisterHedgeTradeAsync(trade);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot hedge portfolio position. Position: {jsonText}. Settings: {settingsJson}",
                    JsonConvert.SerializeObject(positionPortfolio),
                    JsonConvert.SerializeObject(instrumentSettings));

                ex.FailActivity();

                activity?.AddTag("hedge-message", "cannot execute hedge trade");
            }
        }

        private readonly Dictionary<string, ExchangeMarketInfo> _externalMarketInfoCache1 = new();

        private async Task<Dictionary<string, (IExternalMarket, ExchangeMarketInfo, LpHedgeSettings)>> GetAvailableExternalMarkets(PositionPortfolio positionPortfolio,
            LiquidityProviderInstrumentSettings instrumentSettings)
        {
            var externalMarkets = new Dictionary<string, (IExternalMarket, ExchangeMarketInfo, LpHedgeSettings)>();

            HashSet<string> skipMarkets = null;
            lock (_skippedSources) skipMarkets = _skippedSources.Keys.ToHashSet();

            foreach (var source in instrumentSettings.LpHedges.Where(e => 
                                                    e.Mode == EngineMode.Active 
                                                    //&& !skipMarkets.Contains(e.ExternalMarket)
                                                    ))
            {
                var market = _externalMarketManager.GetExternalMarketByName(source.ExternalMarket);

                if (market == null)
                {
                    _logger.LogError(
                        "Cannot found external market '{marketName}' to hedge position {positionId}, symbol: {symbol}. Skip this source",
                        source.ExternalMarket, positionPortfolio.Id, positionPortfolio.Symbol);

                    SkipSource(source.ExternalMarket, "Cannot found external market");

                    continue;
                }

                var mInfo = await GetExchangeMarketInfo(source, market);


                if (mInfo == null)
                {
                    _logger.LogError(
                        "Cannot found symbol info in external market '{marketName}' to hedge position {positionId}, symbol: {symbol}. Skip this source",
                        source.ExternalMarket, positionPortfolio.Id, positionPortfolio.Symbol);

                    lock (_skippedSources)
                        _skippedSources[source.ExternalMarket] =
                            (DateTime.UtcNow, "Cannot found symbol info in external market");

                    continue;
                }

                externalMarkets.Add(source.ExternalMarket, (market, mInfo, source));
            }

            return externalMarkets;
        }

        private async Task<ExchangeMarketInfo> GetExchangeMarketInfo(LpHedgeSettings settings, IExternalMarket market)
        {
            var key = $"{settings.ExternalMarket}--{settings.ExternalSymbol}";
            try
            {
                if (!_externalMarketInfoCache1.TryGetValue(key, out var mInfo))
                {
                    var resp = await market.GetMarketInfoAsync(new MarketRequest() {Market = settings.ExternalSymbol});
                    mInfo = resp.Info;
                    _externalMarketInfoCache1[key] = mInfo;
                }

                return mInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Cannot get market info for key: {key}");
                return null;
            }
        }

        private void SkipSource(string source, string reason)
        {
            lock (_skippedSources)
                _skippedSources[source] = (DateTime.UtcNow, reason);

            _logger.LogWarning("Skipped market. Reason: {reasonText}", reason);
        }

        private async Task<List<LpOrder>> GenerateOrdersFromSourceAsync(IExternalMarket market,
            ExchangeMarketInfo symbolInfo, string source, double useExternalBalancePercentage,
            OrderSide side)
        {
            var externalBook = await _orderBookManager.GetOrderBook(symbolInfo.Market, source);

            if (externalBook == null)
            {
                _logger.LogError("External order book is not found ({externalMarket} - {externalSymbol})", source, symbolInfo.Market);
                Activity.Current?.SetStatus(OpenTelemetry.Trace.Status.Error);
                return new List<LpOrder>();
            }

            var useExternalBalance = (decimal)useExternalBalancePercentage / 100;
            var externalBaseBalance = (double) (_externalBalanceCacheManager.GetBalances(source, symbolInfo.BaseAsset).Free * useExternalBalance);
            var externalQuoteBalance = (double) (_externalBalanceCacheManager.GetBalances(source, symbolInfo.QuoteAsset).Free * useExternalBalance);

            var list = new List<LpOrder>();

            if(side == OrderSide.Buy)
            {
                var baseVolumeTotal = 0.0;

                foreach (var level in externalBook.Asks)
                {
                    var price = level.Price;
                    var volume = level.Volume;

                    if (baseVolumeTotal + volume > externalBaseBalance)
                    {
                        volume = Math.Min(volume, NotNegative(externalBaseBalance - baseVolumeTotal));
                    }

                    volume = Math.Round(volume, Mathematics.AccuracyToNormalizeDouble);
                    volume = Math.Round(volume, symbolInfo.VolumeAccuracy, MidpointRounding.ToZero);

                    if (volume < (double)symbolInfo.MinVolume)
                        continue;

                    list.Add(new LpOrder("", symbolInfo.Market, source, price, volume, OrderSide.Sell));

                    baseVolumeTotal += volume;
                }
            }

            if (side == OrderSide.Sell)
            {
                var quoteVolumeTotal = 0.0;

                foreach (var level in externalBook.Bids)
                {
                    var price = level.Price;
                    var volume = level.Volume;

                    var quoteVolume = price * volume;

                    if (quoteVolumeTotal + quoteVolume > externalQuoteBalance)
                    {
                        volume = Math.Min(volume, NotNegative((externalQuoteBalance - quoteVolumeTotal) / price));
                    }

                    volume = Math.Round(volume, Mathematics.AccuracyToNormalizeDouble);
                    volume = Math.Round(volume, symbolInfo.VolumeAccuracy, MidpointRounding.ToZero);

                    if (volume < (double)symbolInfo.MinVolume)
                        continue;

                    list.Add(new LpOrder("", symbolInfo.Market, source, price, volume, OrderSide.Buy));

                    quoteVolume = price * volume;
                    
                    quoteVolumeTotal += quoteVolume;
                    quoteVolumeTotal = Math.Round(quoteVolumeTotal, Mathematics.AccuracyToNormalizeDouble);
                }
            }


            return list;
        }

        private double NotNegative(double value)
        {
            if (value < 0) return 0;
            return value;
        }

        private string GenerateReferenceId(PositionPortfolio position)
        {
            return $"pos-{position.Id}";
        }
    }
}