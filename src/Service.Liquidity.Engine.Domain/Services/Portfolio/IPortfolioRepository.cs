using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Liquidity.Engine.Domain.Models.Portfolio;

namespace Service.Liquidity.Engine.Domain.Services.Portfolio
{
    public interface IPortfolioRepository
    {
        Task Update(WalletPortfolio portfolio);
        Task<List<WalletPortfolio>> GetAll();
    }
}