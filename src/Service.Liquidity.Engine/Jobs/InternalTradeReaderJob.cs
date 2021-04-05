using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using MyJetWallet.Sdk.Service;
using Service.Liquidity.Engine.Domain.Services.Portfolio;
using Service.Liquidity.Engine.Domain.Services.Wallets;
using Service.TradeHistory.ServiceBus;

namespace Service.Liquidity.Engine.Jobs
{
    public class InternalTradeReaderJob
    {
        private readonly IPortfolioManager _manager;
        private readonly ILpWalletManager _walletManager;

        public InternalTradeReaderJob(
            ISubscriber<IReadOnlyList<WalletTradeMessage>> subscriber, 
            IPortfolioManager manager,
            ILpWalletManager walletManager)
        {
            _manager = manager;
            _walletManager = walletManager;
            subscriber.Subscribe(HandleTrades);
        }

        private async ValueTask HandleTrades(IReadOnlyList<WalletTradeMessage> trades)
        {
            //using var _ = MyTelemetry.StartActivity("Handle event ")

            var wallets = _walletManager.GetAll().Select(e => e.WalletId).ToList();

            var list = trades.Where(e => wallets.Contains(e.WalletId)).ToList();

            if (list.Any())
            {
                using var _ = MyTelemetry.StartActivity("Handle event WalletTradeMessage")?.AddTag("event-count", list.Count)?.AddTag("event-name", "WalletTradeMessage");

                await _manager.RegisterLocalTrades(list);
            }

            
        }
    }
}