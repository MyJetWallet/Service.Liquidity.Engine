using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Liquidity.Engine.Domain.Models.Portfolio;

namespace Service.Liquidity.Engine.Domain.Services.Portfolio
{
    public interface IPortfolioRepository
    {
        Task Update(List<PositionPortfolio> positions);
        Task<List<PositionPortfolio>> GetAll();
    }
}