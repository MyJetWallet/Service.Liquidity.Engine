using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyNoSqlServer.Abstractions;
using Service.Liquidity.Engine.Domain.Models.Portfolio;
using Service.Liquidity.Engine.Domain.NoSql;

namespace Service.Liquidity.Engine.Domain.Services.Portfolio
{
    public class PortfolioMyNoSqlRepository : IPortfolioRepository
    {
        private readonly IMyNoSqlServerDataWriter<WalletPortfolioNoSql> _dataWriter;

        public PortfolioMyNoSqlRepository(IMyNoSqlServerDataWriter<WalletPortfolioNoSql> dataWriter)
        {
            _dataWriter = dataWriter;
        }

        public async Task Update(WalletPortfolio portfolio)
        {
            var item = WalletPortfolioNoSql.Create(portfolio);
            await _dataWriter.InsertOrReplaceAsync(item);
        }

        public async Task<List<WalletPortfolio>> GetAll()
        {
            var data = await _dataWriter.GetAsync();
            return data.Select(e => e.Portfolio).ToList();
        }
    }
}