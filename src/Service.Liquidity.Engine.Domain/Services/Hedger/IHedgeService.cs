using System.Threading.Tasks;

namespace Service.Liquidity.Engine.Domain.Services.Hedger
{
    public interface IHedgeService
    {
        Task HedgePortfolioAsync();
    }
}