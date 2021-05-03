using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using ME.Contracts.Api.IncomingMessages;
using Microsoft.Extensions.Logging;
using MyJetWallet.Domain.Assets;
using MyJetWallet.Domain.Orders;
using MyJetWallet.MatchingEngine.Grpc.Api;
using MyJetWallet.Sdk.Service;
using OpenTelemetry.Trace;
using Service.AssetsDictionary.Client;
using Service.AssetsDictionary.Domain.Models;
using Service.Liquidity.Engine.Domain.Models.LiquidityProvider;
using Service.Liquidity.Engine.Domain.Models.Settings;
using Service.Liquidity.Engine.Domain.Services.ExternalMarkets;
using Service.Liquidity.Engine.Domain.Services.MarketMakers;
using Service.Liquidity.Engine.Domain.Services.OrderBooks;
using Service.Liquidity.Engine.Domain.Services.Settings;
using Service.Liquidity.Engine.Domain.Services.Wallets;


namespace Service.Liquidity.Engine.Domain.Services.LiquidityProvider
{
    public class AggregateLiquidityProvider: IMarketMaker, IAggregateLiquidityProviderOrders
    {
        private readonly ILogger<AggregateLiquidityProvider> _logger;
        private readonly IOrderIdGenerator _orderIdGenerator;
        private readonly IOrderBookManager _orderBookManager;
        private readonly IMarketMakerSettingsAccessor _settingsAccessor;
        private readonly ILpWalletManager _walletManager;
        private readonly ITradingServiceClient _tradingServiceClient;
        private readonly ISpotInstrumentDictionaryClient _instrumentDictionary;
        private readonly IAssetsDictionaryClient _assetsDictionary;
        private readonly IExternalBalanceCacheManager _externalBalanceCacheManager;

        private Dictionary<string, List<LpOrder>> _lastOrders = new ();

        public AggregateLiquidityProvider(ILogger<AggregateLiquidityProvider> logger,
            IOrderIdGenerator orderIdGenerator,
            IOrderBookManager orderBookManager,
            IMarketMakerSettingsAccessor settingsAccessor,
            ILpWalletManager walletManager,
            ITradingServiceClient tradingServiceClient,
            ISpotInstrumentDictionaryClient instrumentDictionary,
            IAssetsDictionaryClient assetsDictionary,
            IExternalBalanceCacheManager externalBalanceCacheManager)
        {
            _logger = logger;
            _orderIdGenerator = orderIdGenerator;
            _orderBookManager = orderBookManager;
            _settingsAccessor = settingsAccessor;
            _walletManager = walletManager;
            _tradingServiceClient = tradingServiceClient;
            _instrumentDictionary = instrumentDictionary;
            _assetsDictionary = assetsDictionary;
            _externalBalanceCacheManager = externalBalanceCacheManager;
        }

        public async Task RefreshOrders()
        {
            using var _ = MyTelemetry.StartActivity("AggregateLiquidityProvider - refresh orders");

            var settings = _settingsAccessor.GetLiquidityProviderSettings();
            var globalSetting = _settingsAccessor.GetMarketMakerSettings();

            var list = new List<Task>();
            foreach (var setting in settings)//.Where(s => s.InstrumentSymbol == "XRPUSD"))
            {
                list.Add(RefreshInstrument(setting, globalSetting));
            }

            await Task.WhenAll(list);

        }

        public List<LpOrder> GetCurrentOrders(string brokerId, string symbol)
        {
            lock (_lastOrders)
            {
                if (_lastOrders.TryGetValue($"{symbol}|{brokerId}", out var data))
                {
                    return data;
                }
            }

            return new List<LpOrder>();
        }

        private async Task RefreshInstrument(LiquidityProviderInstrumentSettings setting, MarketMakerSettings globalSetting)
        {
            using var activity = MyTelemetry.StartActivity("AggregateLiquidityProvider - Refresh Instrument")
                ?.AddTag("instrument-symbol", setting.Symbol)
                ?.AddTag("wallet-name", setting.LpWalletName);

            if (globalSetting.Mode == EngineMode.Disabled || setting.Mode == EngineMode.Disabled)
                return;

            var localWallet = _walletManager.GetWallet(setting.LpWalletName);
            if (localWallet == null)
            {
                _logger.LogError("Cannot handle {symbol} [{wallet}]. Local wallet is not found", setting.Symbol, setting.LpWalletName);
                activity?.SetStatus(OpenTelemetry.Trace.Status.Error);
                return;
            }

            var instrument = _instrumentDictionary.GetSpotInstrumentById(new SpotInstrumentIdentity()
            {
                BrokerId = localWallet.BrokerId,
                Symbol = setting.Symbol
            });

            if (instrument == null)
            {
                _logger.LogError("Cannot handle {symbol} [{wallet}]. Spot instrument do not found",
                    setting.Symbol, setting.LpWalletName);
                activity?.SetStatus(OpenTelemetry.Trace.Status.Error);
                return;
            }

            if (!instrument.IsEnabled && 
                setting.Mode != EngineMode.Disabled &&
                globalSetting.Mode != EngineMode.Disabled)
            {
                _logger.LogError("Can not handle {symbol} [{wallet}]. Spot instrument do DISABLED",
                    setting.Symbol, setting.LpWalletName);
                activity?.SetStatus(OpenTelemetry.Trace.Status.Error);
                return;
            }

            var baseAsset = _assetsDictionary.GetAssetById(new AssetIdentity()
            {
                BrokerId = instrument.BrokerId,
                Symbol = instrument.BaseAsset
            });

            var quoteAsset = _assetsDictionary.GetAssetById(new AssetIdentity()
            {
                BrokerId = instrument.BrokerId,
                Symbol = instrument.QuoteAsset
            });

            if (baseAsset == null)
            {
                _logger.LogError("Cannot handle {symbol} [{wallet}]. Base asset do not found",
                    setting.Symbol, setting.LpWalletName);
                activity?.SetStatus(OpenTelemetry.Trace.Status.Error);
                return;
            }

            if (quoteAsset == null)
            {
                _logger.LogError("Cannot handle {symbol} [{wallet}]. Quote asset do not found",
                    setting.Symbol, setting.LpWalletName);
                activity?.SetStatus(OpenTelemetry.Trace.Status.Error);
                return;
            }

            var localBalances = _walletManager.GetBalances(setting.LpWalletName);
            var baseBalance = localBalances.FirstOrDefault(e => e.AssetId == baseAsset.Symbol)?.Balance ?? 0;
            var quoteBalance = localBalances.FirstOrDefault(e => e.AssetId == quoteAsset.Symbol)?.Balance ?? 0;

            var orderBase = _orderIdGenerator.GenerateBase();

            var externalOrders = new List<LpOrder>();

            foreach (var source in setting.LpSources)
            {
                try
                {
                    if (source.Mode == EngineMode.Disabled)
                        continue;

                    var mode = source.Mode;
                    if (mode == EngineMode.Active)
                        mode = globalSetting.Mode;

                    var list = await GenerateOrdersFromSource(setting.Symbol, source, globalSetting, instrument, baseAsset, quoteAsset, mode);
                    externalOrders.AddRange(list);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Cannot get external order for {symbol} [{wallet}] from source {sourceName}", setting.Symbol, setting.LpWalletName, source.ExternalMarket);
                }
            }

            var orderIndex = 0;

            {
                var baseVolumeTotal = 0.0;

                foreach (var level in externalOrders.Where(o => o.Side == OrderSide.Sell && o.Status == LpOrderStatus.New).OrderBy(o => o.Price))
                {
                    var price = level.Price;
                    var volume = level.Volume;

                    if (baseVolumeTotal + volume > baseBalance)
                    {
                        volume = Math.Min(volume, baseBalance - baseVolumeTotal);
                    }

                    volume = Math.Round(volume, Mathematics.AccuracyToNormalizeDouble);
                    volume = Math.Round(volume, baseAsset.Accuracy, MidpointRounding.ToZero);

                    if (volume < (double)instrument.MinVolume)
                        continue;

                    level.Id = _orderIdGenerator.GenerateOrderId(orderBase, ++orderIndex);
                    level.Status = LpOrderStatus.Todo;
                    
                    baseVolumeTotal += volume;
                }
            }

            {
                var quoteVolumeTotal = 0.0;

                foreach (var level in externalOrders.Where(o => o.Side == OrderSide.Buy && o.Status == LpOrderStatus.New).OrderByDescending(o => o.Price))
                {
                    var price = level.Price;

                    var volume = level.Volume;

                    var quoteVolume = price * volume;
                    quoteVolume = Math.Round(quoteVolume, Mathematics.AccuracyToNormalizeDouble);
                    quoteVolume = Math.Round(quoteVolume, quoteAsset.Accuracy, MidpointRounding.ToPositiveInfinity);

                    if (quoteVolumeTotal + quoteVolume > quoteBalance)
                    {
                        volume = Math.Min(volume, (quoteBalance - quoteVolumeTotal) / price);
                    }

                    volume = Math.Round(volume, Mathematics.AccuracyToNormalizeDouble);
                    volume = Math.Round(volume, baseAsset.Accuracy, MidpointRounding.ToZero);

                    if (volume < (double)instrument.MinVolume)
                        continue;

                    level.Id = _orderIdGenerator.GenerateOrderId(orderBase, ++orderIndex);
                    level.Status = LpOrderStatus.Todo;

                    quoteVolumeTotal += quoteVolume;
                    quoteVolumeTotal = Math.Round(quoteVolumeTotal, Mathematics.AccuracyToNormalizeDouble);
                }
            }

            var request = new MultiLimitOrder()
            {
                Id = orderBase,
                MessageId = orderBase,
                BrokerId = localWallet.BrokerId,
                AccountId = localWallet.ClientId,
                WalletId = localWallet.WalletId,
                AssetPairId = setting.Symbol,
                CancelAllPreviousLimitOrders = true,
                CancelMode = MultiLimitOrder.Types.CancelMode.BothSides,
                Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
                WalletVersion = -1
            };

            foreach (var lpOrder in externalOrders.Where(o => o.Status == LpOrderStatus.Todo))
            {
                lpOrder.Id = _orderIdGenerator.GenerateOrderId(orderBase, ++orderIndex);

                var volume = lpOrder.Side == OrderSide.Buy ? lpOrder.Volume : -lpOrder.Volume;

                request.Orders.Add(new MultiLimitOrder.Types.Order()
                {
                    Id = lpOrder.Id,
                    Price = lpOrder.Price.ToString(CultureInfo.InvariantCulture),
                    Volume = volume.ToString(CultureInfo.InvariantCulture)
                });
            }

            var resp = await PlaceMultiLimitOrderAsync(request);

            HandleMeResponse(setting.Symbol, setting.LpWalletName, resp, activity, externalOrders);

            lock(_lastOrders) _lastOrders[$"{setting.Symbol}|{localWallet.BrokerId}"] = externalOrders;
        }

        private async Task<List<LpOrder>> GenerateOrdersFromSource(string symbol, LpSourceSettings setting, MarketMakerSettings globalSetting, ISpotInstrument instrument, IAsset baseAsset, IAsset quoteAsset, EngineMode mode)
        {
            var externalBook = await _orderBookManager.GetOrderBook(setting.ExternalSymbol, setting.ExternalMarket);

            if (externalBook == null)
            {
                _logger.LogError("External order book is not found ({externalMarket} - {externalSymbol})", setting.ExternalMarket, setting.ExternalSymbol);
                Activity.Current?.SetStatus(OpenTelemetry.Trace.Status.Error);
                return new List<LpOrder>();
            }

            var externalMarketInfo = _externalBalanceCacheManager.GetMarketInfo(setting.ExternalMarket, setting.ExternalSymbol);
            if (externalMarketInfo == null)
            {
                _logger.LogError("External market info do not found: {externalMarket}, {externalSymbol}", setting.ExternalMarket, setting.ExternalSymbol);
                Activity.Current?.SetStatus(OpenTelemetry.Trace.Status.Error);
                return new List<LpOrder>();
            }

            var useExternalBalance = (decimal)globalSetting.UseExternalBalancePercentage / 100;
            var externalBaseBalance = (double)(_externalBalanceCacheManager.GetBalances(setting.ExternalMarket, externalMarketInfo.BaseAsset).Free * useExternalBalance);
            var externalQuoteBalance = (double)(_externalBalanceCacheManager.GetBalances(setting.ExternalMarket, externalMarketInfo.QuoteAsset).Free * useExternalBalance);

            var list = new List<LpOrder>();

            if (setting.Mode == EngineMode.Active || setting.Mode == EngineMode.Idle)
            {
                if (mode == EngineMode.Active && setting.Mode == EngineMode.Idle)
                    mode = EngineMode.Idle;

                {
                    var baseVolumeTotal = 0.0;

                    foreach (var level in externalBook.Asks)
                    {
                        var price = level.Price;
                        price = price + setting.Markup * price;

                        price = Math.Round(price, Mathematics.AccuracyToNormalizeDouble);
                        price = Math.Round(price, instrument.Accuracy, MidpointRounding.ToPositiveInfinity);


                        var volume = level.Volume;

                        if (baseVolumeTotal + volume > externalBaseBalance)
                        {
                            volume = Math.Min(volume, externalBaseBalance - baseVolumeTotal);
                        }


                        if (volume > (double)instrument.MaxVolume)
                            volume = (double)instrument.MaxVolume;

                        if (price * volume > (double)instrument.MaxOppositeVolume)
                        {
                            volume = Math.Min(volume, (double)instrument.MaxOppositeVolume / price);
                        }

                        volume = Math.Round(volume, Mathematics.AccuracyToNormalizeDouble);
                        volume = Math.Round(volume, baseAsset.Accuracy, MidpointRounding.ToZero);

                        if (volume < (double)instrument.MinVolume)
                            continue;

                        list.Add(new LpOrder("", symbol, setting.ExternalMarket, price, volume, OrderSide.Sell)
                        {
                            Status = mode == EngineMode.Active ? LpOrderStatus.New : LpOrderStatus.Idle
                        });
                        
                        baseVolumeTotal += volume;
                    }
                }

                {
                    var quoteVolumeTotal = 0.0;

                    foreach (var level in externalBook.Bids)
                    {
                        var price = level.Price;
                        price = price - setting.Markup * price;

                        price = Math.Round(price, Mathematics.AccuracyToNormalizeDouble);
                        price = Math.Round(price, instrument.Accuracy, MidpointRounding.ToZero);

                        var volume = level.Volume;

                        var quoteVolume = price * volume;
                        quoteVolume = Math.Round(quoteVolume, Mathematics.AccuracyToNormalizeDouble);
                        quoteVolume = Math.Round(quoteVolume, quoteAsset.Accuracy,
                            MidpointRounding.ToPositiveInfinity);

                        if (quoteVolumeTotal + quoteVolume > externalQuoteBalance)
                        {
                            volume = Math.Min(volume, (externalQuoteBalance - quoteVolumeTotal) / price);
                        }

                        if (volume > (double)instrument.MaxVolume)
                            volume = (double)instrument.MaxVolume;

                        if (price * volume > (double)instrument.MaxOppositeVolume)
                        {
                            volume = Math.Min(volume, (double)instrument.MaxOppositeVolume / price);
                        }

                        volume = Math.Round(volume, Mathematics.AccuracyToNormalizeDouble);
                        volume = Math.Round(volume, baseAsset.Accuracy, MidpointRounding.ToZero);

                        if (volume < (double)instrument.MinVolume)
                            continue;

                        list.Add(new LpOrder("", symbol, setting.ExternalMarket, price, volume, OrderSide.Buy)
                        {
                            Status = mode == EngineMode.Active ? LpOrderStatus.New : LpOrderStatus.Idle
                        });

                        quoteVolumeTotal += quoteVolume;
                        quoteVolumeTotal = Math.Round(quoteVolumeTotal, Mathematics.AccuracyToNormalizeDouble);
                    }
                }
            }

            return list;
        }

        private async Task<MultiLimitOrderResponse> PlaceMultiLimitOrderAsync(MultiLimitOrder request)
        {
            MultiLimitOrderResponse resp;
            using var _ = MyTelemetry.StartActivity("Place Multi Limit Order");

            try
            {
                resp = await _tradingServiceClient.MultiLimitOrderAsync(request);
            }
            catch (Exception ex)
            {
                ex.FailActivity();
                request.AddToActivityAsJsonTag("me-request");
                throw;
            }

            return resp;
        }

        private void HandleMeResponse(string symbol, string walletName, MultiLimitOrderResponse resp, Activity activity, List<LpOrder> externalOrders)
        {
            if (resp.Status != ME.Contracts.Api.IncomingMessages.Status.Ok)
            {
                _logger.LogError(
                    "[{symbol}|{wallet}] Error from ME on Place MultiLimitOrder: {statusText}, {reasonText}",
                    symbol, walletName, resp.Status.ToString(), resp.StatusReason);
                activity?.SetStatus(OpenTelemetry.Trace.Status.Error);

                foreach (var lpOrder in externalOrders)
                {
                    lpOrder.Status = LpOrderStatus.Error;
                    lpOrder.Message = resp.StatusReason;
                    lpOrder.MeStatus = resp.Status.ToString();
                }
                return;
            }
            
            var results = resp.Statuses.GroupBy(e => e.Status);
            foreach (var result in results)
            {
                if (result.Key == ME.Contracts.Api.IncomingMessages.Status.Ok)
                    _logger.LogInformation("[{symbol}|{wallet}] Success place {count} orders", symbol, walletName, result.Count());
                else
                    _logger.LogInformation("[{symbol}|{wallet}] Cannot place {count} orders, status: {statusText}", symbol, walletName, result.Count(), result.Key.ToString());

                foreach (var statuse in result)
                {
                    var lpOrder = externalOrders.FirstOrDefault(e => e.Id == statuse.Id);
                    if (lpOrder == null) continue;

                    if (statuse.Status != ME.Contracts.Api.IncomingMessages.Status.Ok)
                    {
                        lpOrder.Status = LpOrderStatus.Error;
                        lpOrder.Message = statuse.StatusReason;
                        lpOrder.MeStatus = statuse.Status.ToString();
                    }
                    else
                    {
                        lpOrder.Status = LpOrderStatus.Placed;
                    }
                }
            }
        }
    }
}