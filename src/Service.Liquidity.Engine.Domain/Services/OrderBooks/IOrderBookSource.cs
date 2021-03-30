using System.Collections.Generic;
using MyJetWallet.Domain.Prices;
using Service.Liquidity.Engine.Domain.Models.OrderBooks;

namespace Service.Liquidity.Engine.Domain.Services.OrderBooks
{
    public interface IOrderBookSource
    {
        string GetName();
        List<string> GetSymbols();
        bool HasSymbol(string symbol);
        LeOrderBook GetOrderBook(string symbol);
    }
}