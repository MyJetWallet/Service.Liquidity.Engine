using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Service.Liquidity.Engine.Domain.Models.Portfolio;
using Service.Liquidity.Engine.Domain.Services.Wallets;
using Service.TradeHistory.ServiceBus;

namespace Service.Liquidity.Engine.Domain.Services.Portfolio
{
    public class PortfolioManager : IPortfolioManager, IStartable
    {
        private readonly ILogger<PortfolioManager> _logger;
        private readonly IPortfolioRepository _repository;
        private readonly ILpWalletManager _walletManager;
        private Dictionary<string, WalletPortfolio> _data = new();

        public PortfolioManager(
            ILogger<PortfolioManager> logger,
            IPortfolioRepository repository, 
            ILpWalletManager walletManager)
        {
            _logger = logger;
            _repository = repository;
            _walletManager = walletManager;
        }

        public async Task RegisterLocalTrades(List<WalletTradeMessage> trades)
        {
            var toUpdate = new Dictionary<string, WalletPortfolio>();

            foreach (var trade in trades)
            {
                var portfolio = GetPortfolioByWalletId(trade.WalletId);

                if (portfolio.Positions.Any(e => e.OpenTrade.TradeUId == trade.Trade.TradeUId))
                    continue;

                var position = new PositionPortfolio()
                {
                    OpenTrade = trade.Trade
                };

                portfolio.Positions.Add(position);

                toUpdate[portfolio.WalletId] = portfolio;

                _logger.LogInformation("Register a new trade in portfolio. WalletName: {walletName}. Position: {jsonText}", 
                    portfolio.WalletName, JsonConvert.SerializeObject(position));
            }

            if (toUpdate.Any())
            {
                var list = toUpdate.Select(e => _repository.Update(e.Value)).ToList();
                await Task.WhenAll(list);
            }
        }

        public Task<WalletPortfolio> GetPortfolioByWalletName(string walletName)
        {
            var portfolio = _data.Values.FirstOrDefault(e => e.WalletName == walletName);
            return Task.FromResult(portfolio);
        }

        public void Start()
        {
            var data = _repository.GetAll().GetAwaiter().GetResult();

            _data = data.ToDictionary(e => e.WalletId);
        }

        private WalletPortfolio GetPortfolioByWalletId(string walletId)
        {
            if (_data.TryGetValue(walletId, out var portfolio))
                return portfolio;

            var wallet = _walletManager.GetAll().FirstOrDefault(e => e.WalletId == walletId);

            if (wallet == null)
            {
                _logger.LogError("Do not found wallet with walletId = {walletId}", walletId);
                return null;
            }

            return new WalletPortfolio()
            {
                BrokerId = wallet.BrokerId,
                ClientId = wallet.ClientId,
                WalletId = wallet.WalletId,
                WalletName = wallet.Name,
                Positions = new List<PositionPortfolio>()
            };
        }
    }
}