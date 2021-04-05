using System.Threading.Tasks;
using MyJetWallet.Sdk.Service;
using Service.Liquidity.Engine.Domain.Models.Portfolio;
using Service.Liquidity.Engine.Domain.Services.Portfolio;

namespace Service.Liquidity.Engine.Domain.Services.Hedger
{
    public class HedgeService : IHedgeService
    {
        private readonly IPortfolioManager _portfolioManager;

        public HedgeService(IPortfolioManager portfolioManager)
        {
            _portfolioManager = portfolioManager;
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
            activity?.AddTag("positionId", positionPortfolio.Id).AddTag("symbol", positionPortfolio.Symbol);


        }
    }
}