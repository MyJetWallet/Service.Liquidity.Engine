using System.Collections.Generic;
using System.Threading.Tasks;
using MyJetWallet.Domain.ExternalMarketApi.Models;

namespace Service.Liquidity.Engine.Domain.Services.OrderBooks
{
    public interface IOrderBookManager
    {
        Task<LeOrderBook> GetOrderBook(string symbol, string source);

        Task<Dictionary<string, List<string>>> GetSourcesAndSymbols();

        Task<List<string>> GetSymbols(string source);

        Task<List<string>> GetSourcesWithSymbol(string symbol);

        Task<List<string>> GetSources();
    }
}