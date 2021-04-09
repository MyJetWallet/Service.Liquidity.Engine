using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using ME.Contracts.Api.IncomingMessages;
using Microsoft.Extensions.Logging;
using MyJetWallet.Domain.Assets;
using MyJetWallet.MatchingEngine.Grpc.Api;
using MyJetWallet.Sdk.Service;
using Newtonsoft.Json;
using OpenTelemetry.Trace;
using Service.AssetsDictionary.Client;
using Service.Balances.Domain.Models;
using Service.Liquidity.Engine.Domain.Models.Settings;
using Service.Liquidity.Engine.Domain.Services.OrderBooks;
using Service.Liquidity.Engine.Domain.Services.Settings;
using Service.Liquidity.Engine.Domain.Services.Wallets;
using Status = ME.Contracts.Api.IncomingMessages.Status;

namespace Service.Liquidity.Engine.Domain.Services.MarketMakers
{
    public static class Mathematics
    {
        public static int AccuracyToNormalizeDouble { get; set; } = 12;
    }

    public class MirroringLiquidityProvider: IMarketMaker
    {
        private readonly ILogger<MirroringLiquidityProvider> _logger;
        private readonly IOrderIdGenerator _orderIdGenerator;
        private readonly IOrderBookManager _orderBookManager;
        private readonly IMarketMakerSettingsAccessor _settingsAccessor;
        private readonly ILpWalletManager _walletManager;
        private readonly ITradingServiceClient _tradingServiceClient;
        private readonly ISpotInstrumentDictionaryClient _instrumentDictionary;
        private readonly IAssetsDictionaryClient _assetsDictionary;

        public MirroringLiquidityProvider(
            ILogger<MirroringLiquidityProvider> logger,
            IOrderIdGenerator orderIdGenerator,
            IOrderBookManager orderBookManager,
            IMarketMakerSettingsAccessor settingsAccessor,
            ILpWalletManager walletManager,
            ITradingServiceClient tradingServiceClient,
            ISpotInstrumentDictionaryClient instrumentDictionary,
            IAssetsDictionaryClient assetsDictionary)
        {
            _logger = logger;
            _orderIdGenerator = orderIdGenerator;
            _orderBookManager = orderBookManager;
            _settingsAccessor = settingsAccessor;
            _walletManager = walletManager;
            _tradingServiceClient = tradingServiceClient;
            _instrumentDictionary = instrumentDictionary;
            _assetsDictionary = assetsDictionary;
        }

        public async Task RefreshOrders()
        {
            using var _ = MyTelemetry.StartActivity("Market maker refresh orders");
            Console.WriteLine("Market maker refresh orders ...");

            var settings = _settingsAccessor.GetMirroringLiquiditySettingsList();
            var globalSetting = _settingsAccessor.GetMarketMakerSettings();

            var list = new List<Task>();
            foreach (var setting in settings)
            {
                list.Add(RefreshInstrument(setting, globalSetting));
            }

            await Task.WhenAll(list);
        }

        private async Task RefreshInstrument(MirroringLiquiditySettings setting, MarketMakerSettings globalSetting)
        {
            using var activity = MyTelemetry.StartActivity("Refresh Instrument")
                ?.AddTag("instrument-symbol", setting.InstrumentSymbol)
                ?.AddTag("wallet-name", setting.WalletName);

            try
            {

                var externalBook = await _orderBookManager.GetOrderBook(setting.ExternalSymbol, setting.ExternalMarket);

                if (externalBook == null)
                {
                    _logger.LogError("Cannot handle {symbol} [{wallet}]. External order book is not found ({externalMarket} - {externalSymbol})",
                        setting.InstrumentSymbol, setting.WalletName, setting.ExternalMarket, setting.ExternalSymbol);
                    activity?.SetStatus(OpenTelemetry.Trace.Status.Error);
                    return;
                }

                var localWallet = _walletManager.GetWallet(setting.WalletName);
                if (localWallet == null)
                {
                    _logger.LogError("Cannot handle {symbol} [{wallet}]. Local wallet is not found",
                        setting.InstrumentSymbol, setting.WalletName);
                    activity?.SetStatus(OpenTelemetry.Trace.Status.Error);
                    return;
                }

                var instrument = _instrumentDictionary.GetSpotInstrumentById(new SpotInstrumentIdentity()
                {
                    BrokerId = localWallet.BrokerId,
                    Symbol = setting.InstrumentSymbol
                });

                if (instrument == null)
                {
                    _logger.LogError("Cannot handle {symbol} [{wallet}]. Spot instrument do not found",
                        setting.InstrumentSymbol, setting.WalletName);
                    activity?.SetStatus(OpenTelemetry.Trace.Status.Error);
                    return;
                }

                if (!instrument.IsEnabled && setting.Mode == EngineMode.Active &&
                    globalSetting.Mode == EngineMode.Active)
                {
                    _logger.LogError("Can not handle {symbol} [{wallet}]. Spot instrument do DISABLED",
                        setting.InstrumentSymbol, setting.WalletName);
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
                        setting.InstrumentSymbol, setting.WalletName);
                    activity?.SetStatus(OpenTelemetry.Trace.Status.Error);
                    return;
                }

                if (quoteAsset == null)
                {
                    _logger.LogError("Cannot handle {symbol} [{wallet}]. Quote asset do not found",
                        setting.InstrumentSymbol, setting.WalletName);
                    activity?.SetStatus(OpenTelemetry.Trace.Status.Error);
                    return;
                }

                var localBalances = _walletManager.GetBalances(setting.WalletName);
                var baseBalance = localBalances.FirstOrDefault(e => e.AssetId == baseAsset.Symbol)?.Balance ?? 0;
                var quoteBalance = localBalances.FirstOrDefault(e => e.AssetId == quoteAsset.Symbol)?.Balance ?? 0;


                var orderBase = _orderIdGenerator.GenerateBase();

                var request = new MultiLimitOrder()
                {
                    Id = orderBase,
                    MessageId = orderBase,
                    BrokerId = localWallet.BrokerId,
                    AccountId = localWallet.ClientId,
                    WalletId = localWallet.WalletId,
                    AssetPairId = setting.InstrumentSymbol,
                    CancelAllPreviousLimitOrders = true,
                    CancelMode = MultiLimitOrder.Types.CancelMode.BothSides,
                    Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
                    WalletVersion = -1
                };

                var orderIndex = 0;


                if (globalSetting.Mode == EngineMode.Active && setting.Mode == EngineMode.Active &&
                    instrument.IsEnabled)
                {
                    {
                        var baseVolumeTotal = 0.0;

                        foreach (var level in externalBook.Asks)
                        {
                            var price = level.Price;
                            price = price + setting.Markup * price;

                            price = Math.Round(price, Mathematics.AccuracyToNormalizeDouble);
                            price = Math.Round(price, instrument.Accuracy, MidpointRounding.ToPositiveInfinity);


                            var volume = level.Volume;

                            if (baseVolumeTotal + volume > baseBalance)
                            {
                                volume = baseBalance - baseVolumeTotal;
                            }

                            if (baseVolumeTotal + volume > setting.MaxSellSideVolume)
                            {
                                volume = setting.MaxSellSideVolume - baseVolumeTotal;
                            }

                            if (volume < (double) instrument.MinVolume)
                                continue;

                            if (volume > (double) instrument.MaxVolume)
                                volume = (double) instrument.MaxVolume;

                            if (price * volume > (double) instrument.MaxOppositeVolume)
                            {
                                volume = (double) instrument.MaxOppositeVolume / price;
                            }

                            volume = Math.Round(volume, Mathematics.AccuracyToNormalizeDouble);
                            volume = Math.Round(volume, baseAsset.Accuracy, MidpointRounding.ToZero);

                            request.Orders.Add(new MultiLimitOrder.Types.Order()
                            {
                                Id = _orderIdGenerator.GenerateOrderId(orderBase, ++orderIndex),
                                Price = price.ToString(CultureInfo.InvariantCulture),
                                Volume = (-volume).ToString(CultureInfo.InvariantCulture)
                            });

                            baseVolumeTotal += volume;
                        }
                    }

                    {
                        var baseVolumeTotal = 0.0;
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

                            if (quoteVolumeTotal + quoteVolume > quoteBalance)
                            {
                                volume = (quoteBalance - quoteVolumeTotal) / price;
                            }

                            if (baseVolumeTotal + volume > setting.MaxBuySideVolume)
                            {
                                volume = setting.MaxBuySideVolume - baseVolumeTotal;
                            }

                            if (volume < (double) instrument.MinVolume)
                                continue;

                            if (volume > (double) instrument.MaxVolume)
                                volume = (double) instrument.MaxVolume;

                            if (price * volume > (double) instrument.MaxOppositeVolume)
                            {
                                volume = (double) instrument.MaxOppositeVolume / price;
                            }

                            volume = Math.Round(volume, Mathematics.AccuracyToNormalizeDouble);
                            volume = Math.Round(volume, baseAsset.Accuracy, MidpointRounding.ToZero);

                            request.Orders.Add(new MultiLimitOrder.Types.Order()
                            {
                                Id = _orderIdGenerator.GenerateOrderId(orderBase, ++orderIndex),
                                Price = price.ToString(CultureInfo.InvariantCulture),
                                Volume = volume.ToString(CultureInfo.InvariantCulture)
                            });

                            quoteVolumeTotal += quoteVolume;
                            quoteVolumeTotal = Math.Round(quoteVolumeTotal, Mathematics.AccuracyToNormalizeDouble);

                            baseVolumeTotal += volume;
                        }
                    }
                }


                var resp = await PlaceMultiLimitOrderAsync(request);

                HandleMeResponse(setting, resp, activity);
            }
            catch (Exception ex)
            {
                ex.FailActivity();
                setting.AddToActivityAsJsonTag("mm-settings");
                _logger.LogError(ex, "Exception on RefreshInstrument. Settings: {jsonText}", JsonConvert.SerializeObject(setting));
            }
        }

        private void HandleMeResponse(MirroringLiquiditySettings setting, MultiLimitOrderResponse resp, Activity activity)
        {
            if (resp.Status != Status.Ok)
            {
                _logger.LogError(
                    "[{symbol}|{wallet}] Error from ME on Place MultiLimitOrder: {statusText}, {reasonText}",
                    setting.InstrumentSymbol, setting.WalletName, resp.Status.ToString(), resp.StatusReason);
                activity?.SetStatus(OpenTelemetry.Trace.Status.Error);
            }

            var results = resp.Statuses.GroupBy(e => e.Status);

            foreach (var result in results)
            {
                if (result.Key == Status.Ok)
                    _logger.LogInformation("[{symbol}|{wallet}] Success place {count} orders",
                        setting.InstrumentSymbol, setting.WalletName, result.Count());
                else
                    _logger.LogInformation("[{symbol}|{wallet}] Cannot place {count} orders, status: {statusText}",
                        setting.InstrumentSymbol, setting.WalletName, result.Count(), result.Key.ToString());
            }
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
    }
}