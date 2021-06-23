using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Sdk.Service.Tools;
using Service.BalanceHistory.ServiceBus;
using Service.Liquidity.Engine.Domain.Services.Hedger;
using Service.Liquidity.Engine.Domain.Services.Portfolio;
using Service.Liquidity.Engine.Domain.Services.Settings;
using Service.Liquidity.Engine.Domain.Services.Wallets;

namespace Service.Liquidity.Engine.Jobs
{
    public class InternalTradeReaderJob: IStartable, IDisposable
    {
        private readonly IPortfolioManager _manager;
        private readonly IHedgeService _hedgeService;
        private readonly ILpWalletManager _walletManager;
        private readonly IHedgeSettingsManager _hedgeSettings;
        private readonly MyTaskTimer _hedgeTimer;

        private readonly object _sync = new object();
        private bool _needToHedge = false;

        private int _countInterval = 100;

        public InternalTradeReaderJob(
            ILogger<InternalTradeReaderJob> logger,
            ISubscriber<IReadOnlyList<WalletTradeMessage>> subscriber, 
            IPortfolioManager manager,
            IHedgeService hedgeService,
            ILpWalletManager walletManager,
            IHedgeSettingsManager hedgeSettings)
        {
            _manager = manager;
            _hedgeService = hedgeService;
            _walletManager = walletManager;
            _hedgeSettings = hedgeSettings;
            subscriber.Subscribe(HandleTrades);

            _hedgeTimer = new MyTaskTimer(nameof(InternalTradeReaderJob), TimeSpan.FromMilliseconds(5000), logger, DoHedge).DisableTelemetry();
        }

        private async Task DoHedge()
        {
            _hedgeTimer.ChangeInterval(TimeSpan.FromMilliseconds(_hedgeSettings.GetGlobalHedgeSettings().HedgeTimerIntervalMSec));

            _countInterval++;

            lock (_sync)
            {
                if (!_needToHedge && _countInterval < 10)
                    return;

                _needToHedge = false;
                _countInterval = 0;
            }

            await _hedgeService.HedgePortfolioAsync();
        }

        private async ValueTask HandleTrades(IReadOnlyList<WalletTradeMessage> trades)
        {
            var wallets = _walletManager.GetAll().Select(e => e.WalletId).ToList();

            var list = trades.Where(e => wallets.Contains(e.WalletId)).ToList();

            if (list.Any())
            {
                using var _ = MyTelemetry.StartActivity("Handle event WalletTradeMessage")
                    ?.AddTag("event-count", list.Count)?.AddTag("event-name", "WalletTradeMessage");

                await _manager.RegisterLocalTradesAsync(list);

                lock (_sync) _needToHedge = true;
            }
        }

        public void Start()
        {
            _hedgeTimer.Start();
        }

        public void Dispose()
        {
            _hedgeTimer.Stop();
            _hedgeTimer.Dispose();
        }
    }
}