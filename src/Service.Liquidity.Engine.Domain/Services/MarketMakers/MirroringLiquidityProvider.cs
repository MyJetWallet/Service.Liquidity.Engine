using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using ME.Contracts.Api.IncomingMessages;
using Microsoft.Extensions.Logging;
using MyJetWallet.MatchingEngine.Grpc.Api;
using Service.Liquidity.Engine.Domain.Models.Settings;
using Service.Liquidity.Engine.Domain.Services.OrderBooks;
using Service.Liquidity.Engine.Domain.Services.Settings;
using Service.Liquidity.Engine.Domain.Services.Wallets;

namespace Service.Liquidity.Engine.Domain.Services.MarketMakers
{
    public class MirroringLiquidityProvider: IMarketMaker
    {
        private readonly ILogger<MirroringLiquidityProvider> _logger;
        private readonly IOrderIdGenerator _orderIdGenerator;
        private readonly IOrderBookManager _orderBookManager;
        private readonly IMarketMakerSettingsAccessor _settingsAccessor;
        private readonly ILpWalletManager _walletManager;
        private readonly ITradingServiceClient _tradingServiceClient;

        public MirroringLiquidityProvider(
            ILogger<MirroringLiquidityProvider> logger,
            IOrderIdGenerator orderIdGenerator,
            IOrderBookManager orderBookManager,
            IMarketMakerSettingsAccessor settingsAccessor,
            ILpWalletManager walletManager,
            ITradingServiceClient tradingServiceClient)
        {
            _logger = logger;
            _orderIdGenerator = orderIdGenerator;
            _orderBookManager = orderBookManager;
            _settingsAccessor = settingsAccessor;
            _walletManager = walletManager;
            _tradingServiceClient = tradingServiceClient;
        }

        public async Task RefreshOrders()
        {
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
            var externalBook = _orderBookManager.GetOrderBook(setting.ExternalSymbol, setting.ExternalMarket);

            if (externalBook == null)
            {
                _logger.LogError("Cannot handle {symbol} [{wallet}]. External order book is not found", setting.InstrumentSymbol, setting.WalletName);
                return;
            }

            var localWallet = _walletManager.GetWallet(setting.WalletName);
            if (localWallet == null)
            {
                _logger.LogError("Cannot handle {symbol} [{wallet}]. Local wallet is not found", setting.InstrumentSymbol, setting.WalletName);
                return;
            }

            var localBalances = _walletManager.GetBalances(setting.WalletName);

            if (!localBalances.Any())
            {
                _logger.LogError("Cannot handle {symbol} [{wallet}]. Local balances is not found", setting.InstrumentSymbol, setting.WalletName);
                return;
            }

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

            if (globalSetting.Mode == EngineMode.Active && setting.Mode == EngineMode.Active)
            {
                foreach (var level in externalBook.Asks)
                {
                    request.Orders.Add(new MultiLimitOrder.Types.Order()
                    {
                        Id = _orderIdGenerator.GenerateOrderId(orderBase, ++orderIndex),
                        Price = level.Price.ToString(CultureInfo.InvariantCulture),
                        Volume = (-level.Volume).ToString(CultureInfo.InvariantCulture)
                    });
                }

                foreach (var level in externalBook.Bids)
                {
                    request.Orders.Add(new MultiLimitOrder.Types.Order()
                    {
                        Id = _orderIdGenerator.GenerateOrderId(orderBase, ++orderIndex),
                        Price = level.Price.ToString(CultureInfo.InvariantCulture),
                        Volume = level.Volume.ToString(CultureInfo.InvariantCulture)
                    });
                }
            }


            var resp = await _tradingServiceClient.MultiLimitOrderAsync(request);

            if (resp.Status != Status.Ok)
            {
                _logger.LogError("[{symbol}|{wallet}] Error from ME on Place MultiLimitOrder: {statusText}, {reasonText}",
                    setting.InstrumentSymbol, setting.WalletName, resp.Status.ToString(), resp.StatusReason);
            }

            var results = resp.Statuses.GroupBy(e => e.Status);

            foreach (var result in results)
            {
                if (result.Key == Status.Ok)
                    _logger.LogInformation("[{symbol}|{wallet}] Success place {count} orders", setting.InstrumentSymbol, setting.WalletName, result.Count());
                else
                    _logger.LogInformation("[{symbol}|{wallet}] Cannot place {count} orders, status: {statusText}", setting.InstrumentSymbol, setting.WalletName, result.Count(), result.Key.ToString());
            }
        }
    }
}