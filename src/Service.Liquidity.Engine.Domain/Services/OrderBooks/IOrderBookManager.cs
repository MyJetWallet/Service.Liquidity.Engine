using System.Collections.Generic;
using MyJetWallet.Domain.Prices;
using Service.Liquidity.Engine.Domain.Models.OrderBooks;

namespace Service.Liquidity.Engine.Domain.Services.OrderBooks
{
    public interface IOrderBookManager
    {
        LeOrderBook GetOrderBook(string symbol, string source);

        Dictionary<string, List<string>> GetSourcesAndSymbols();

        List<string> GetSymbols(string source);

        List<string> GetSourcesWithSymbol(string symbol);

        List<string> GetSources();
    }
}