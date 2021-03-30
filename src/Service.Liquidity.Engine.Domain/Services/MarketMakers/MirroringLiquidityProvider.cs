using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.MatchingEngine.Grpc.Api;
using Service.Liquidity.Engine.Domain.Services.OrderBooks;
using Service.Liquidity.Engine.Domain.Services.Settings;
using Service.Liquidity.Engine.Domain.Services.Wallets;

namespace Service.Liquidity.Engine.Domain.Services.MarketMakers
{
    public class MirroringLiquidityProvider: IMarketMaker
    {
        private readonly ILogger<MirroringLiquidityProvider> _logger;
        private readonly IOrderBookManager _orderBookManager;
        private readonly IMarketMakerSettingsAccessor _settingsAccessor;
        private readonly ILpWalletManager _walletManager;
        private readonly ITradingServiceClient _tradingServiceClient;

        public MirroringLiquidityProvider(
            ILogger<MirroringLiquidityProvider> logger,
            IOrderBookManager orderBookManager,
            IMarketMakerSettingsAccessor settingsAccessor,
            ILpWalletManager walletManager,
            ITradingServiceClient tradingServiceClient)
        {
            _logger = logger;
            _orderBookManager = orderBookManager;
            _settingsAccessor = settingsAccessor;
            _walletManager = walletManager;
            _tradingServiceClient = tradingServiceClient;
        }

        public Task RefreshOrders()
        {
            throw new System.NotImplementedException();
        }
    }
}