using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Service.Liquidity.Engine.Domain.Models.Portfolio;
using Service.Liquidity.Engine.Domain.Services.Portfolio;

namespace Service.Liquidity.Engine.Tests.Mock
{
    public class PortfolioRepositoryMock : IPortfolioRepository
    {
        public Dictionary<string, PositionPortfolio> Data = new();

        public Task Update(List<PositionPortfolio> positions)
        {
            foreach (var position in positions.Where(e => !e.IsOpen))
            {
                Data.Remove(position.Id);
            }
            foreach (var position in positions.Where(e => e.IsOpen))
            {
                Data[position.Id] = position;
            }
            return Task.CompletedTask;
        }

        public Task<List<PositionPortfolio>> GetAll()
        {
            return Task.FromResult(Data.Values.ToList());
        }
    }
}