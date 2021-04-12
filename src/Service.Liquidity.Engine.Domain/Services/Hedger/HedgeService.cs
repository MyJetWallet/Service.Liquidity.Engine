using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Domain.ExternalMarketApi;
using MyJetWallet.Domain.ExternalMarketApi.Dto;
using MyJetWallet.Domain.Orders;
using MyJetWallet.Sdk.Service;
using Newtonsoft.Json;
using OpenTelemetry.Trace;
using Service.Liquidity.Engine.Domain.Models.Portfolio;
using Service.Liquidity.Engine.Domain.Models.Settings;
using Service.Liquidity.Engine.Domain.Services.ExternalMarkets;
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
        private readonly IHedgeInstrumentSettingsManager _instrumentSettingsManager;
        private readonly IExternalMarketManager _externalMarketManager;
        private readonly ILpWalletManager _lpWalletManager;

        public HedgeService(
            ILogger<HedgeService> logger,
            IPortfolioManager portfolioManager,
            IHedgeSettingsManager settingsManager,
            IHedgeInstrumentSettingsManager instrumentSettingsManager,
            IExternalMarketManager externalMarketManager,
            ILpWalletManager lpWalletManager
            )
        {
            _logger = logger;
            _portfolioManager = portfolioManager;
            _settingsManager = settingsManager;
            _instrumentSettingsManager = instrumentSettingsManager;
            _externalMarketManager = externalMarketManager;
            _lpWalletManager = lpWalletManager;
        }

        public async Task HedgePortfolioAsync()
        {
            using var _ = MyTelemetry.StartActivity("Hedge portfolio");

            var portfolio = await _portfolioManager.GetPortfolioAsync();

            foreach (var positionPortfolio in portfolio)
            {
                await HedgePositionAsync(positionPortfolio);
            }
        }

        private async Task HedgePositionAsync(PositionPortfolio positionPortfolio)
        {
            using var activity = MyTelemetry.StartActivity("Hedge portfolio position");
            
            activity?.AddTag("positionId", positionPortfolio.Id)
                     .AddTag("symbol", positionPortfolio.Symbol);

            var settings = _settingsManager.GetGlobalHedgeSettings();
            var instrumentSettings = _instrumentSettingsManager.GetHedgeInstrumentSettings(positionPortfolio.Symbol, positionPortfolio.WalletId);

            if (settings == null || instrumentSettings == null)
            {
                activity?.AddTag("hedge-mode", "no-settings");
                return;
            }

            if (settings.Mode != EngineMode.Active)
            {
                activity?.AddTag("hedge-mode", settings.Mode.ToString());
                activity?.AddTag("hedge-message", "global disabled");
                return;
            }

            if (instrumentSettings.Mode != EngineMode.Active)
            {
                activity?.AddTag("hedge-mode", instrumentSettings.Mode.ToString());
                activity?.AddTag("hedge-message", "instrument disabled");
                return;
            }

            var wallet = _lpWalletManager.GetWalletById(instrumentSettings.WalletId);
            if (wallet == null)
            {
                _logger.LogError("Cannot hedge position {positionId}, because wallet '{marketName}' not found", positionPortfolio.Id, instrumentSettings.WalletId);
                activity?.AddTag("hedge-message", "lp wallet not found");
                activity?.SetStatus(Status.Error);
                return;
            }

            activity?.AddTag("walletName", wallet.Name);
            
            var market = _externalMarketManager.GetExternalMarketByName(instrumentSettings.ExternalMarket);

            if (market == null)
            {
                activity?.AddTag("hedge-message", "external market do not found");
                _logger.LogError("Cannot hedge position {positionId}, because external market '{marketName}' not found", positionPortfolio.Id, instrumentSettings.ExternalMarket);
                activity?.SetStatus(Status.Error);
                return;
            }

            activity?.AddTag("external-market", instrumentSettings.ExternalMarket);

            var hedgeSide = positionPortfolio.Side == OrderSide.Buy ? OrderSide.Sell : OrderSide.Buy;
            var hedgeVolume = (double)(-positionPortfolio.BaseVolume);
            var hedgeReferenceId = GenerateReferenceId(positionPortfolio);

            _logger.LogInformation("Try to hedge position: {jsonText}", JsonConvert.SerializeObject(positionPortfolio));

            try
            {
                new { instrumentSettings.ExternalMarket, instrumentSettings.ExternalSymbol, hedgeSide, hedgeVolume, hedgeReferenceId }.AddToActivityAsJsonTag("external-trade-request");

                var trade = await market.MarketTrade(new MarketTradeRequest()
                {
                    Side = hedgeSide,
                    Volume = hedgeVolume,
                    Market = instrumentSettings.ExternalSymbol,
                    ReferenceId = hedgeReferenceId
                });

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

                activity?.AddTag("hedge-result", "success");

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Cannot hedge portfolio position. Position: {jsonText}. Settings: {settingsJson}", 
                    JsonConvert.SerializeObject(positionPortfolio),
                    JsonConvert.SerializeObject(instrumentSettings));

                ex.FailActivity();

                activity?.AddTag("hedge-message", "cannot execute hedge trade");
            }

        }

        private string GenerateReferenceId(PositionPortfolio position)
        {
            return $"pos:{position.Id}";
        }
    }
}