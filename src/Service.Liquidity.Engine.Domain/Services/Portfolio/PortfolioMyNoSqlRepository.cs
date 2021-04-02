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
        private readonly IMyNoSqlServerDataWriter<PositionPortfolioNoSql> _dataWriter;

        public PortfolioMyNoSqlRepository(IMyNoSqlServerDataWriter<PositionPortfolioNoSql> dataWriter)
        {
            _dataWriter = dataWriter;
        }

        public async Task Update(List<PositionPortfolio> positions)
        {
            var items = positions
                .Where(e => e.IsOpen)
                .Select(PositionPortfolioNoSql.Create)
                .ToList();

            if (items.Any())
                await _dataWriter.BulkInsertOrReplaceAsync(items);

            items = positions
                .Where(e => !e.IsOpen)
                .Select(PositionPortfolioNoSql.Create)
                .ToList();


            var list = items.Select(e => _dataWriter.DeleteAsync(e.PartitionKey, e.RowKey).AsTask()).ToList();

            if (list.Any())
                await Task.WhenAll(list);
        }

        public async Task<List<PositionPortfolio>> GetAll()
        {
            var data = await _dataWriter.GetAsync();
            return data.Select(e => e.Position).ToList();
        }
    }
}